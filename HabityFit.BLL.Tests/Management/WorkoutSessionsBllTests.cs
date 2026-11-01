using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._0.Common;
using HabityFit.BLL._09.WorkoutSessionsManagement;
using HabityFit.BLL._09.WorkoutSessionsManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL.Tests.Management
{
    public class WorkoutSessionsBllTests
    {
        private static HabityFitDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<HabityFitDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new HabityFitDbContext(options);
        }

        public WorkoutSessionsBllTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        private readonly IMapper _mapper;

        #region SyncWorkoutSession
        [Fact]
        public async Task SyncWorkoutSession_WhenSessionBelongsToAnotherUser_ReturnsFalseAndKeepsVictimData()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var attackerId = Guid.NewGuid();
            var victimId = Guid.NewGuid();
            var sessionDate = DateTime.UtcNow.AddDays(-1);
            var sessionId = Guid.NewGuid();
            var existingSession = new WorkoutSessions
            {
                Id = sessionId,
                UserId = victimId,
                Date = sessionDate,
                DurationMinutes = 60,
                Notes = "Original notes",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.WorkoutSessions.Add(existingSession);
            await context.SaveChangesAsync();

            var sessionBll = new WorkoutSessionsBll(context, _mapper);
            var dto = new WorkoutSessionsDto
            {
                Id = sessionId,
                UserId = attackerId,
                Date = sessionDate,
                DurationMinutes = 5,
                Notes = "Changed by attacker",
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await sessionBll.SyncWorkoutSession(dto, attackerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeFalse();
            var persisted = await context.WorkoutSessions.SingleAsync();
            persisted.UserId.Should().Be(victimId);
            persisted.DurationMinutes.Should().Be(60);
            persisted.Notes.Should().Be("Original notes");
        }

        [Fact]
        public async Task SyncWorkoutSession_WhenNewSessionHasForeignUserId_PersistsAuthenticatedUserAsOwner()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var attackerId = Guid.NewGuid();
            var victimId = Guid.NewGuid();
            var sessionBll = new WorkoutSessionsBll(context, _mapper);
            var dto = new WorkoutSessionsDto
            {
                Id = Guid.NewGuid(),
                UserId = victimId,
                Date = DateTime.UtcNow.AddDays(-1),
                DurationMinutes = 45,
                Notes = "Created by attacker",
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await sessionBll.SyncWorkoutSession(dto, attackerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.WorkoutSessions.SingleAsync();
            persisted.UserId.Should().Be(attackerId);
        }

        [Fact]
        public async Task SyncWorkoutSession_WhenOwnerSendsSameDate_UpdatesExistingSession()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var ownerId = Guid.NewGuid();
            var sessionDate = DateTime.UtcNow.AddDays(-1);
            var existingSession = new WorkoutSessions
            {
                Id = Guid.NewGuid(),
                UserId = ownerId,
                Date = sessionDate,
                DurationMinutes = 60,
                Notes = "Original notes",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.WorkoutSessions.Add(existingSession);
            await context.SaveChangesAsync();

            var sessionBll = new WorkoutSessionsBll(context, _mapper);
            var dto = new WorkoutSessionsDto
            {
                Id = Guid.NewGuid(),
                UserId = ownerId,
                Date = sessionDate,
                DurationMinutes = 45,
                Notes = "Updated notes",
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await sessionBll.SyncWorkoutSession(dto, ownerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.WorkoutSessions.SingleAsync();
            persisted.Id.Should().Be(existingSession.Id);
            persisted.UserId.Should().Be(ownerId);
            persisted.DurationMinutes.Should().Be(45);
            persisted.Notes.Should().Be("Updated notes");
        }

        [Fact]
        public async Task SyncWorkoutSession_WhenDtoTriesToChangeOwner_KeepsPersistedOwner()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var sessionDate = DateTime.UtcNow.AddDays(-1);
            var existingSession = new WorkoutSessions
            {
                Id = Guid.NewGuid(),
                UserId = ownerId,
                Date = sessionDate,
                DurationMinutes = 60,
                Notes = "Original notes",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.WorkoutSessions.Add(existingSession);
            await context.SaveChangesAsync();

            var sessionBll = new WorkoutSessionsBll(context, _mapper);
            var dto = new WorkoutSessionsDto
            {
                Id = existingSession.Id,
                UserId = otherUserId,
                Date = sessionDate,
                DurationMinutes = 30,
                Notes = "Updated notes",
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await sessionBll.SyncWorkoutSession(dto, ownerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.WorkoutSessions.SingleAsync();
            persisted.UserId.Should().Be(ownerId);
            persisted.DurationMinutes.Should().Be(30);
        }
        #endregion
    }
}
