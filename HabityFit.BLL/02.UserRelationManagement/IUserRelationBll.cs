using HabityFit.BLL._02.UserRelationManagement.Dto;

namespace HabityFit.BLL._02.UserRelationManagement
{
    public interface IUserRelationBll
    {
        Task<bool> SyncUserRelation(UserRelationDto dto, Guid userId);
        Task<bool> CreateFriendshipAsync(Guid userId, Guid friendId);
    }
}
