namespace HabityFit.BLL._03.BodyMeasurementsManagement.Dto
{
    public interface IBodyMeasurementsBll
    {
        Task<bool> SyncBody(BodyMeasurementsDto dto, Guid userId);
    }
}
