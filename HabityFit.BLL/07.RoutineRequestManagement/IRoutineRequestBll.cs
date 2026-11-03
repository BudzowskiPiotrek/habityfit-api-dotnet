using HabityFit.BLL._07.RoutineRequestManagement.Dto;

namespace HabityFit.BLL._07.RoutineRequestManagement
{
    public interface IRoutineRequestBll
    {
        Task<bool> SyncRoutineRequest(RoutineRequestDto dto, Guid userId);
    }
}
