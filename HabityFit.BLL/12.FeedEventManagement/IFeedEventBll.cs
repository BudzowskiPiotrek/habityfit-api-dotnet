using HabityFit.BLL._12.FeedEventManagement.Dto;

namespace HabityFit.BLL._12.FeedEventManagement
{
    public interface IFeedEventBll
    {
        Task<bool> SyncFeedEvent(FeedEventDto dto, Guid userId);
    }
}
