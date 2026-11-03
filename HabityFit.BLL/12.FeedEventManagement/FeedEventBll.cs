using AutoMapper;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._12.FeedEventManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL._12.FeedEventManagement
{
    public class FeedEventBll : IFeedEventBll
    {
        #region Fields
        private readonly IMapper _mapper;
        private readonly HabityFitDbContext _context;
        #endregion

        #region Constructors
        public FeedEventBll(HabityFitDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        #endregion

        #region Methods
        public async Task<bool> SyncFeedEvent(FeedEventDto dto, Guid userId)
        {
            var existing = await _context.FeedEvents.FirstOrDefaultAsync(fe => fe.Id == dto.Id);

            if (existing == null)
            {
                var entity = _mapper.Map<FeedEvent>(dto);
                entity.UserId = userId;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.FeedEvents.AddAsync(entity);
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
                    _context.FeedEvents.Update(existing);
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