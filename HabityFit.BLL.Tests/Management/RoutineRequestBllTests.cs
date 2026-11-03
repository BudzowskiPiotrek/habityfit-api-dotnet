using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._0.Common;
using HabityFit.BLL._07.RoutineRequestManagement;
using HabityFit.BLL._07.RoutineRequestManagement.Dto;
using HabityFit.BLL._07.RoutinesManagement;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL.Tests.Management
{
    public class RoutineRequestBllTests
    {
        private static HabityFitDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<HabityFitDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new HabityFitDbContext(options);
        }

        public RoutineRequestBllTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        private readonly IMapper _mapper;

        #region SyncRoutineRequest
        [Fact]
        public async Task SyncRoutineRequest_WhenNewRequestHasForeignSenderId_UsesAuthenticatedUserAsSender()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var attackerId = Guid.NewGuid();
            var victimId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var routine = new Routines
            {
                Id = Guid.NewGuid(),
                UserId = attackerId,
                Name = "Attacker routine",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.Routines.Add(routine);
            await context.SaveChangesAsync();

            var requestBll = new RoutineRequestBll(new RoutineBll(context, _mapper), context, _mapper);
            var dto = new RoutineRequestDto
            {
                Id = Guid.NewGuid(),
                SenderId = victimId,
                ReceiverId = receiverId,
                RoutineId = routine.Id,
                Status = RequestStatus.Pending,
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await requestBll.SyncRoutineRequest(dto, attackerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.RoutineRequests.SingleAsync();
            persisted.SenderId.Should().Be(attackerId);
            persisted.Status.Should().Be(RequestStatus.Pending);
        }

        [Fact]
        public async Task SyncRoutineRequest_WhenSenderUsesAnotherUsersRoutine_ReturnsFalse()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var attackerId = Guid.NewGuid();
            var victimId = Guid.NewGuid();
            var victimRoutine = new Routines
            {
                Id = Guid.NewGuid(),
                UserId = victimId,
                Name = "Victim routine",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            context.Routines.Add(victimRoutine);
            await context.SaveChangesAsync();

            var requestBll = new RoutineRequestBll(new RoutineBll(context, _mapper), context, _mapper);
            var dto = new RoutineRequestDto
            {
                Id = Guid.NewGuid(),
                SenderId = attackerId,
                ReceiverId = victimId,
                RoutineId = victimRoutine.Id,
                Status = RequestStatus.Pending,
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await requestBll.SyncRoutineRequest(dto, attackerId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeFalse();
            context.RoutineRequests.Should().BeEmpty();
        }

        [Fact]
        public async Task SyncRoutineRequest_WhenSenderTriesToAcceptOwnRequest_ReturnsFalse()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var routine = new Routines
            {
                Id = Guid.NewGuid(),
                UserId = senderId,
                Name = "Sender routine",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            var request = new RoutineRequests
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                RoutineId = routine.Id,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Sender = null!, // InMemory does not enforce foreign keys.
                Receiver = null!,
                Routine = null!
            };
            context.Routines.Add(routine);
            context.RoutineRequests.Add(request);
            await context.SaveChangesAsync();

            var requestBll = new RoutineRequestBll(new RoutineBll(context, _mapper), context, _mapper);
            var dto = new RoutineRequestDto
            {
                Id = request.Id,
                SenderId = senderId,
                ReceiverId = receiverId,
                RoutineId = routine.Id,
                Status = RequestStatus.Accepted,
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await requestBll.SyncRoutineRequest(dto, senderId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeFalse();
            var persisted = await context.RoutineRequests.SingleAsync();
            persisted.Status.Should().Be(RequestStatus.Pending);
            context.Routines.Count(r => r.UserId == receiverId).Should().Be(0);
        }

        [Fact]
        public async Task SyncRoutineRequest_WhenReceiverAcceptsPendingRequest_ClonesRoutineOnlyOnce()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var routine = new Routines
            {
                Id = Guid.NewGuid(),
                UserId = senderId,
                Name = "Sender routine",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            var request = new RoutineRequests
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                RoutineId = routine.Id,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Sender = null!, // InMemory does not enforce foreign keys.
                Receiver = null!,
                Routine = null!
            };
            context.Routines.Add(routine);
            context.RoutineRequests.Add(request);
            await context.SaveChangesAsync();

            var requestBll = new RoutineRequestBll(new RoutineBll(context, _mapper), context, _mapper);
            var dto = new RoutineRequestDto
            {
                Id = request.Id,
                SenderId = senderId,
                ReceiverId = receiverId,
                RoutineId = routine.Id,
                Status = RequestStatus.Accepted,
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var firstResult = await requestBll.SyncRoutineRequest(dto, receiverId);

            dto.UpdatedAt = DateTime.UtcNow.AddMinutes(1);
            await requestBll.SyncRoutineRequest(dto, receiverId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            firstResult.Should().BeTrue();
            var persisted = await context.RoutineRequests.SingleAsync();
            persisted.Status.Should().Be(RequestStatus.Accepted);
            context.Routines.Count(r => r.UserId == receiverId).Should().Be(1);
        }

        [Fact]
        public async Task SyncRoutineRequest_WhenSenderTriesToChangeReceiverOrRoutine_ReturnsFalse()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var anotherReceiverId = Guid.NewGuid();
            var routine = new Routines
            {
                Id = Guid.NewGuid(),
                UserId = senderId,
                Name = "Sender routine",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            var anotherRoutine = new Routines
            {
                Id = Guid.NewGuid(),
                UserId = senderId,
                Name = "Another sender routine",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                User = null! // InMemory does not enforce foreign keys.
            };
            var request = new RoutineRequests
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                RoutineId = routine.Id,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Sender = null!, // InMemory does not enforce foreign keys.
                Receiver = null!,
                Routine = null!
            };
            context.Routines.AddRange(routine, anotherRoutine);
            context.RoutineRequests.Add(request);
            await context.SaveChangesAsync();

            var requestBll = new RoutineRequestBll(new RoutineBll(context, _mapper), context, _mapper);
            var dto = new RoutineRequestDto
            {
                Id = request.Id,
                SenderId = senderId,
                ReceiverId = anotherReceiverId,
                RoutineId = anotherRoutine.Id,
                Status = RequestStatus.Pending,
                UpdatedAt = DateTime.UtcNow
            };
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await requestBll.SyncRoutineRequest(dto, senderId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeFalse();
            var persisted = await context.RoutineRequests.SingleAsync();
            persisted.ReceiverId.Should().Be(receiverId);
            persisted.RoutineId.Should().Be(routine.Id);
        }
        #endregion
    }
}
