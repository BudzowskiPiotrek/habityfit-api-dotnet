using AutoMapper;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._03.BodyMeasurementsManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL._03.BodyMeasurementsManagement
{
    public class BodyMeasurementsBll : IBodyMeasurementsBll
    {
        #region Fields
        private readonly IMapper _mapper;
        private readonly HabityFitDbContext _context;
        #endregion


        #region Constructors
        public BodyMeasurementsBll(HabityFitDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        #endregion


        #region Methods
        public async Task<bool> SyncBody(BodyMeasurementsDto dto, Guid userId)
        {
            var existing = await _context.BodyMeasurements
                .FirstOrDefaultAsync(b => b.Id == dto.Id);

            if (existing != null && existing.IdUser != userId)
                return false;

            existing ??= await _context.BodyMeasurements
                .FirstOrDefaultAsync(b => b.IdUser == userId && b.Date == dto.Date);

            if (existing == null)
            {
                var entity = _mapper.Map<BodyMeasurements>(dto);
                entity.IdUser = userId;
                await _context.BodyMeasurements.AddAsync(entity);
            }
            else
            {
                var existingId = existing.Id;
                var ownerId = existing.IdUser;
                if (existing.DeletedAt != null && dto.DeletedAt == null)
                {
                    existing.DeletedAt = null;
                }

                if (dto.UpdatedAt > existing.UpdatedAt || existing.UpdatedAt == null)
                {
                    _mapper.Map(dto, existing);
                    existing.Id = existingId;
                    existing.IdUser = ownerId;
                    _context.BodyMeasurements.Update(existing);
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
