using HabityFit.BLL._07.RoutinesManagement.Dto;

namespace HabityFit.BLL._07.RoutinesManagement
{
    public interface IRoutineBll
    {
        Task<bool> SyncRoutine(RoutineDto routineDto, Guid userId);
        Task<bool> CloneRoutineForUserAsync(Guid routineId, Guid userId);
    }
}
