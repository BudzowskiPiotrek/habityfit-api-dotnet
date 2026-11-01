using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._0.Common;
using HabityFit.BLL._07.RoutinesManagement;
using HabityFit.BLL._07.RoutinesManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL.Tests.Management
{
    public class RoutineBllTests
    {
        private static HabityFitDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<HabityFitDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new HabityFitDbContext(options);
        }

        public RoutineBllTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        private readonly IMapper _mapper;

        #region SyncRoutine
        [Fact]
        public async Task SyncRoutine_WhenRoutineBelongsToAnotherUser_ReturnsFalseAndKeepsVictimData()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var attackerId = Guid.NewGuid();
            var victimId = Guid.NewGuid();
            var routineId = Guid.NewGuid();
            var existingRoutine = new Routines
            {
                Id = routineId,
                UserId = victimId,
                Name = "Victim routine",
                Description = "Original description",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.Routines.Add(existingRoutine);
            await context.SaveChangesAsync();

            var routineBll = new RoutineBll(context, _mapper);
            var dto = new RoutineDto
            {
                Id = routineId,
                UserId = attackerId,
                Name = "Hijacked routine",
                Description = "Changed by attacker",
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await routineBll.SyncRoutine(dto, attackerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeFalse();
            var persisted = await context.Routines.SingleAsync();
            persisted.UserId.Should().Be(victimId);
            persisted.Name.Should().Be("Victim routine");
            persisted.Description.Should().Be("Original description");
        }

        [Fact]
        public async Task SyncRoutine_WhenNewRoutineHasForeignUserId_PersistsAuthenticatedUserAsOwner()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var attackerId = Guid.NewGuid();
            var victimId = Guid.NewGuid();
            var routineBll = new RoutineBll(context, _mapper);
            var dto = new RoutineDto
            {
                Id = Guid.NewGuid(),
                UserId = victimId,
                Name = "New routine",
                Description = "Created by attacker",
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await routineBll.SyncRoutine(dto, attackerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.Routines.SingleAsync();
            persisted.UserId.Should().Be(attackerId);
        }

        [Fact]
        public async Task SyncRoutine_WhenDtoTriesToChangeOwner_KeepsPersistedOwner()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var routineId = Guid.NewGuid();
            var existingRoutine = new Routines
            {
                Id = routineId,
                UserId = ownerId,
                Name = "Own routine",
                Description = "Original description",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.Routines.Add(existingRoutine);
            await context.SaveChangesAsync();

            var routineBll = new RoutineBll(context, _mapper);
            var dto = new RoutineDto
            {
                Id = routineId,
                UserId = otherUserId,
                Name = "Updated routine",
                Description = "Updated description",
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await routineBll.SyncRoutine(dto, ownerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.Routines.SingleAsync();
            persisted.UserId.Should().Be(ownerId);
            persisted.Name.Should().Be("Updated routine");
            persisted.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task SyncRoutine_WhenOwnerUpdatesRoutine_UpdatesIt()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var ownerId = Guid.NewGuid();
            var routineId = Guid.NewGuid();
            var existingRoutine = new Routines
            {
                Id = routineId,
                UserId = ownerId,
                Name = "Own routine",
                Description = "Original description",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.Routines.Add(existingRoutine);
            await context.SaveChangesAsync();

            var routineBll = new RoutineBll(context, _mapper);
            var dto = new RoutineDto
            {
                Id = routineId,
                UserId = ownerId,
                Name = "Updated routine",
                Description = "Updated description",
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await routineBll.SyncRoutine(dto, ownerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.Routines.SingleAsync();
            persisted.UserId.Should().Be(ownerId);
            persisted.Name.Should().Be("Updated routine");
            persisted.Description.Should().Be("Updated description");
        }
        #endregion
    }
}
