using AutoMapper;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._09.WorkoutSessionsManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL._09.WorkoutSessionsManagement
{
    public class WorkoutSessionsBll : IWorkoutSessionsBll
    {
        #region Fields
        private readonly IMapper _mapper;
        private readonly HabityFitDbContext _context;
        #endregion


        #region Constructors
        public WorkoutSessionsBll(HabityFitDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        #endregion


        #region Methods
        public async Task<bool> SyncWorkoutSession(WorkoutSessionsDto dto, Guid userId)
        {
            var existing = await _context.WorkoutSessions
                .FirstOrDefaultAsync(w => w.Id == dto.Id);

            if (existing != null && existing.UserId != userId)
                return false;

            existing ??= await _context.WorkoutSessions
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Date == dto.Date);

            if (existing == null)
            {
                var entity = _mapper.Map<WorkoutSessions>(dto);
                entity.UserId = userId;
                await _context.WorkoutSessions.AddAsync(entity);
            }
            else
            {
                var existingId = existing.Id;
                var ownerId = existing.UserId;
                if (existing.DeletedAt != null && dto.DeletedAt == null)
                {
                    existing.DeletedAt = null;
                }

                if (dto.UpdatedAt > existing.UpdatedAt || existing.UpdatedAt == null)
                {
                    _mapper.Map(dto, existing);
                    existing.Id = existingId;
                    existing.UserId = ownerId;
                    _context.WorkoutSessions.Update(existing);
                }
                else
                {
                    return true;
                }
            }
            return await _context.SaveChangesAsync() > 0;
        }
        #endregion
    }
}