using AutoMapper;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._07.RoutinesManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL._07.RoutinesManagement
{
    public class RoutineBll : IRoutineBll
    {
        #region Fields
        private readonly IMapper _mapper;
        private readonly HabityFitDbContext _context;
        #endregion


        #region Constructors
        public RoutineBll(HabityFitDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        #endregion


        #region Methods
        public async Task<bool> SyncRoutine(RoutineDto dto, Guid userId)
        {
            var existing = await _context.Routines.FirstOrDefaultAsync(r => r.Id == dto.Id);
            if (existing == null)
            {
                var entity = _mapper.Map<Routines>(dto);
                entity.UserId = userId;
                await _context.Routines.AddAsync(entity);
            }
            else
            {
                if (existing.UserId != userId)
                    return false;

                if (existing.DeletedAt != null && dto.DeletedAt == null)
                {
                    existing.DeletedAt = null;
                }
                if (dto.UpdatedAt > existing.UpdatedAt || existing.UpdatedAt == null)
                {
                    var ownerId = existing.UserId;
                    _mapper.Map(dto, existing);
                    existing.UserId = ownerId;
                    _context.Routines.Update(existing);
                }
                else
                {
                    return true;
                }
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CloneRoutineForUserAsync(Guid routineId, Guid userId)
        {
            try
            {
                var existing = await _context.Routines.AsNoTracking().FirstOrDefaultAsync(r => r.Id == routineId);

                if (existing == null) return false;

                var dateNow = DateTime.UtcNow;

                var cloneRoutine = _mapper.Map<Routines>(existing);
                cloneRoutine.Id = Guid.NewGuid();
                cloneRoutine.UserId = userId;
                cloneRoutine.CreatedAt = dateNow;
                cloneRoutine.UpdatedAt = dateNow;

                await _context.Routines.AddAsync(cloneRoutine);

                var exercises = await _context.RoutineExercises.AsNoTracking().Where(re => re.RoutineId == routineId).ToListAsync();

                foreach (var exercise in exercises)
                {
                    var cloneExercise = _mapper.Map<RoutineExercises>(exercise);
                    cloneExercise.Id = Guid.NewGuid();
                    cloneExercise.RoutineId = cloneRoutine.Id;
                    cloneExercise.CreatedAt = dateNow;
                    cloneExercise.UpdatedAt = dateNow;
                    await _context.RoutineExercises.AddAsync(cloneExercise);
                }

                return await _context.SaveChangesAsync() > 0;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
    }
}
