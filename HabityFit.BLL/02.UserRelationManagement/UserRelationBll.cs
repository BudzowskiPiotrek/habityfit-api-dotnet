using HabityFit.BLL._02.UserRelationManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;
using Microsoft.EntityFrameworkCore;

namespace HabityFit.BLL._02.UserRelationManagement
{
    public class UserRelationBll : IUserRelationBll
    {
        #region Fields
        private readonly HabityFitDbContext _context;
        #endregion


        #region Constructors
        public UserRelationBll(HabityFitDbContext context)
        {
            _context = context;
        }
        #endregion


        #region Methods
        public async Task<bool> SyncUserRelation(UserRelationDto dto, Guid userId)
        {
            if (userId == Guid.Empty)
                return false;

            var existing = await _context.UsersRelations
                .FirstOrDefaultAsync(r => r.Id == dto.Id ||
                                         (r.UserId == dto.UserId && r.FriendId == dto.FriendId) ||
                                         (r.UserId == dto.FriendId && r.FriendId == dto.UserId) ||
                                         (r.UserId == userId && r.FriendId == dto.FriendId) ||
                                         (r.UserId == dto.FriendId && r.FriendId == userId));

            var now = DateTime.UtcNow;

            if (existing is null)
            {
                if (dto.FriendId == Guid.Empty || dto.FriendId == userId)
                    return false;

                var entity = new UsersRelations
                {
                    Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                    UserId = userId,
                    FriendId = dto.FriendId,
                    Status = RelationStatus.Pending,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await _context.UsersRelations.AddAsync(entity);
                return await _context.SaveChangesAsync() > 0;
            }

            var isSender = existing.UserId == userId;
            var isReceiver = existing.FriendId == userId;

            if (!isSender && !isReceiver)
                return false;

            if (existing.Status == RelationStatus.Pending && isReceiver &&
                dto.Status is RelationStatus.Accepted or RelationStatus.Rejected)
            {
                existing.Status = dto.Status;
                existing.UpdatedAt = now;
            }
            else if (dto.DeletedAt is not null)
            {
                existing.DeletedAt = now;
                existing.UpdatedAt = now;
            }
            else
            {
                return false;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CreateFriendshipAsync(Guid userId, Guid friendId)
        {
            var now = DateTime.UtcNow;
            var existing = await _context.UsersRelations.FirstOrDefaultAsync(r =>
            (r.UserId == userId && r.FriendId == friendId) ||
            (r.FriendId == userId && r.UserId == friendId));

            if (existing == null)
            {
                var newRelationship = new UsersRelations
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = now,
                    UpdatedAt = now,
                    UserId = userId,
                    FriendId = friendId,
                    Status = RelationStatus.Accepted,
                };
                _context.UsersRelations.Add(newRelationship);
                return await _context.SaveChangesAsync() > 0;
            }

            if (existing.Status == RelationStatus.Accepted)
                throw new InvalidOperationException("This relation already exists");

            existing.Status = RelationStatus.Accepted;
            existing.UpdatedAt = now;
            existing.DeletedAt = null;
            return await _context.SaveChangesAsync() > 0;
        }
        #endregion
    }
}
