using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._0.Common;
using HabityFit.BLL._03.BodyMeasurementsManagement;
using HabityFit.BLL._03.BodyMeasurementsManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL.Tests.Management
{
    public class BodyMeasurementsBllTests
    {
        private static HabityFitDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<HabityFitDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new HabityFitDbContext(options);
        }

        public BodyMeasurementsBllTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        private readonly IMapper _mapper;

        #region SyncBody
        [Fact]
        public async Task SyncBody_WhenMeasurementBelongsToAnotherUser_ReturnsFalseAndKeepsVictimData()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var attackerId = Guid.NewGuid();
            var victimId = Guid.NewGuid();
            var measurementDate = DateTime.UtcNow.AddDays(-1);
            var measurementId = Guid.NewGuid();
            var existingMeasurement = new BodyMeasurements
            {
                Id = measurementId,
                IdUser = victimId,
                Date = measurementDate,
                Weight = 80,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.BodyMeasurements.Add(existingMeasurement);
            await context.SaveChangesAsync();

            var bodyBll = new BodyMeasurementsBll(context, _mapper);
            var dto = new BodyMeasurementsDto
            {
                Id = measurementId,
                IdUser = attackerId,
                Date = measurementDate,
                Weight = 99,
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await bodyBll.SyncBody(dto, attackerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeFalse();
            var persisted = await context.BodyMeasurements.SingleAsync();
            persisted.IdUser.Should().Be(victimId);
            persisted.Weight.Should().Be(80);
        }

        [Fact]
        public async Task SyncBody_WhenNewMeasurementHasForeignIdUser_PersistsAuthenticatedUserAsOwner()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var attackerId = Guid.NewGuid();
            var victimId = Guid.NewGuid();
            var bodyBll = new BodyMeasurementsBll(context, _mapper);
            var dto = new BodyMeasurementsDto
            {
                Id = Guid.NewGuid(),
                IdUser = victimId,
                Date = DateTime.UtcNow.AddDays(-1),
                Weight = 75,
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await bodyBll.SyncBody(dto, attackerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.BodyMeasurements.SingleAsync();
            persisted.IdUser.Should().Be(attackerId);
        }

        [Fact]
        public async Task SyncBody_WhenOwnerSendsSameDate_UpdatesExistingMeasurement()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var ownerId = Guid.NewGuid();
            var measurementDate = DateTime.UtcNow.AddDays(-1);
            var existingMeasurement = new BodyMeasurements
            {
                Id = Guid.NewGuid(),
                IdUser = ownerId,
                Date = measurementDate,
                Weight = 80,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.BodyMeasurements.Add(existingMeasurement);
            await context.SaveChangesAsync();

            var bodyBll = new BodyMeasurementsBll(context, _mapper);
            var dto = new BodyMeasurementsDto
            {
                Id = Guid.NewGuid(),
                IdUser = ownerId,
                Date = measurementDate,
                Weight = 75,
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await bodyBll.SyncBody(dto, ownerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.BodyMeasurements.SingleAsync();
            persisted.Id.Should().Be(existingMeasurement.Id);
            persisted.IdUser.Should().Be(ownerId);
            persisted.Weight.Should().Be(75);
        }

        [Fact]
        public async Task SyncBody_WhenDtoTriesToChangeOwner_KeepsPersistedOwner()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var measurementDate = DateTime.UtcNow.AddDays(-1);
            var existingMeasurement = new BodyMeasurements
            {
                Id = Guid.NewGuid(),
                IdUser = ownerId,
                Date = measurementDate,
                Weight = 80,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.BodyMeasurements.Add(existingMeasurement);
            await context.SaveChangesAsync();

            var bodyBll = new BodyMeasurementsBll(context, _mapper);
            var dto = new BodyMeasurementsDto
            {
                Id = existingMeasurement.Id,
                IdUser = otherUserId,
                Date = measurementDate,
                Weight = 90,
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await bodyBll.SyncBody(dto, ownerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.BodyMeasurements.SingleAsync();
            persisted.IdUser.Should().Be(ownerId);
            persisted.Weight.Should().Be(90);
        }
        #endregion
    }
}
