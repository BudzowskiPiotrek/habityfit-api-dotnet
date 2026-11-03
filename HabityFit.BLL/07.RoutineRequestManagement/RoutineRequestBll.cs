using AutoMapper;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._07.RoutineRequestManagement.Dto;
using HabityFit.BLL._07.RoutinesManagement;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL._07.RoutineRequestManagement
{
    public class RoutineRequestBll : IRoutineRequestBll
    {
        #region Fields
        private readonly IMapper _mapper;
        private readonly HabityFitDbContext _context;
        private readonly IRoutineBll _routine;
        #endregion


        #region Constructors
        public RoutineRequestBll(IRoutineBll routine, HabityFitDbContext context, IMapper mapper)
        {
            _context = context;
            _routine = routine;
            _mapper = mapper;
        }
        #endregion


        #region Methods
        public async Task<bool> SyncRoutineRequest(RoutineRequestDto dto, Guid userId)
        {
            var existing = await _context.RoutineRequests
                .FirstOrDefaultAsync(r => r.Id == dto.Id ||
                                         (r.SenderId == userId &&
                                          r.ReceiverId == dto.ReceiverId &&
                                          r.RoutineId == dto.RoutineId));

            if (existing == null)
            {
                if (dto.ReceiverId == Guid.Empty || dto.ReceiverId == userId)
                    return false;

                var ownsRoutine = await _context.Routines
                    .AnyAsync(r => r.Id == dto.RoutineId && r.UserId == userId);

                if (!ownsRoutine)
                    return false;

                var entity = _mapper.Map<RoutineRequests>(dto);
                entity.SenderId = userId;
                entity.Status = RequestStatus.Pending;
                await _context.RoutineRequests.AddAsync(entity);
                return await _context.SaveChangesAsync() > 0;
            }

            var isSender = existing.SenderId == userId;
            var isReceiver = existing.ReceiverId == userId;

            if (!isSender && !isReceiver)
                return false;

            if (existing.SenderId != dto.SenderId ||
                existing.ReceiverId != dto.ReceiverId ||
                existing.RoutineId != dto.RoutineId)
            {
                return false;
            }

            if (existing.Status == RequestStatus.Rejected &&
                isSender &&
                dto.Status == RequestStatus.Pending)
            {
                existing.Status = RequestStatus.Pending;
                existing.DeletedAt = null;
                existing.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }

            if (existing.UpdatedAt != null &&
                (dto.UpdatedAt == null || dto.UpdatedAt <= existing.UpdatedAt))
            {
                return true;
            }

            if (dto.DeletedAt is not null)
            {
                if (existing.DeletedAt is null)
                {
                    existing.DeletedAt = DateTime.UtcNow;
                    existing.UpdatedAt = DateTime.UtcNow;
                    return await _context.SaveChangesAsync() > 0;
                }

                return true;
            }

            if (dto.Status == existing.Status)
                return true;

            if (existing.Status != RequestStatus.Pending || !isReceiver)
                return false;

            if (dto.Status == RequestStatus.Accepted)
            {
                var ownsRoutine = await _context.Routines
                    .AnyAsync(r => r.Id == existing.RoutineId && r.UserId == existing.SenderId);

                if (!ownsRoutine)
                    return false;

                if (!await _routine.CloneRoutineForUserAsync(existing.RoutineId, existing.ReceiverId))
                    return false;
            }
            else if (dto.Status != RequestStatus.Rejected)
            {
                return false;
            }

            existing.Status = dto.Status;
            existing.UpdatedAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }
        #endregion
    }
}
