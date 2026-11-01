using HabityFit.BLL._09.WorkoutSessionsManagement.Dto;

namespace HabityFit.BLL._09.WorkoutSessionsManagement
{
    public interface IWorkoutSessionsBll
    {
        Task<bool> SyncWorkoutSession(WorkoutSessionsDto dto, Guid userId);
    }
}
