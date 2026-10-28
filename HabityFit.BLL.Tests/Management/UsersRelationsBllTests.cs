using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._02.UserRelationManagement;
using HabityFit.BLL._02.UserRelationManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL.Tests.Management
{
    public class UsersRelationsBllTests
    {
        private static HabityFitDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<HabityFitDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new HabityFitDbContext(options);
        }

        #region CreateFriendshipAsync
        [Fact]
        public async Task CreateFriendshipAsync_WithTwoValidUsers__ReturnsTrueAndPersistsRelation()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var senderUser = new Users
            {
                Id = Guid.NewGuid(),
                Email = "sender@example.com",
                FullName = "Sender User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,

            };
            var receiverUser = new Users
            {
                Id = Guid.NewGuid(),
                Email = "receiver@example.com",
                FullName = "Receiver User",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTime.UtcNow,
            };
            context.Users.Add(senderUser);
            context.Users.Add(receiverUser);
            await context.SaveChangesAsync();

            var relationBll = new UserRelationBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await relationBll.CreateFriendshipAsync(senderUser.Id, receiverUser.Id);
            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.UsersRelations.FirstOrDefaultAsync(r => r.UserId == senderUser.Id && r.FriendId == receiverUser.Id);
            persisted.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateFriendshipAsync_WhenRelationshipExists_ThrowsInvalidOperationException()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            // InMemory does not validate FKs — for this test, we only care that the relationship exists between these two GUIDs.
            var relation = new UsersRelations
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UserId = Guid.NewGuid(),
                FriendId = Guid.NewGuid(),
                Status = RelationStatus.Accepted
            };
            context.UsersRelations.Add(relation);
            await context.SaveChangesAsync();

            var relationBll = new UserRelationBll(context);
            //---------------------------------------------------------------------------------------------------------------- Act
            Func<Task> act = async () => await relationBll.CreateFriendshipAsync(relation.UserId, relation.FriendId);
            //---------------------------------------------------------------------------------------------------------------- Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        #endregion

        #region SyncUserRelation
        [Fact]
        public async Task SyncUserRelation_WhenNewRelationIsSentAsAccepted_StoresPendingForJwtUser()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var attackerId = Guid.NewGuid();
            var victimId = Guid.NewGuid();
            var relationBll = new UserRelationBll(context);

            var dto = new UserRelationDto
            {
                Id = Guid.NewGuid(),
                UserId = victimId,              // client tries to choose the sender
                FriendId = victimId,            // client tries to become friends with the victim
                Status = RelationStatus.Accepted,
                UpdatedAt = DateTime.UtcNow.AddYears(1)
            };

            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await relationBll.SyncUserRelation(dto, attackerId);

            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.UsersRelations.SingleAsync();
            persisted.UserId.Should().Be(attackerId);
            persisted.FriendId.Should().Be(victimId);
            persisted.Status.Should().Be(RelationStatus.Pending);
            persisted.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task SyncUserRelation_WhenReceiverAcceptsPendingRequest_StoresAccepted()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var relation = new UsersRelations
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = senderId,
                FriendId = receiverId,
                Status = RelationStatus.Pending
            };
            context.UsersRelations.Add(relation);
            await context.SaveChangesAsync();

            var relationBll = new UserRelationBll(context);
            var dto = new UserRelationDto
            {
                Id = relation.Id,
                UserId = senderId,
                FriendId = receiverId,
                Status = RelationStatus.Accepted
            };

            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await relationBll.SyncUserRelation(dto, receiverId);

            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.UsersRelations.SingleAsync();
            persisted.Status.Should().Be(RelationStatus.Accepted);
        }

        [Fact]
        public async Task SyncUserRelation_WhenReceiverRejectsPendingRequest_StoresRejected()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var relation = new UsersRelations
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = senderId,
                FriendId = receiverId,
                Status = RelationStatus.Pending
            };
            context.UsersRelations.Add(relation);
            await context.SaveChangesAsync();

            var relationBll = new UserRelationBll(context);
            var dto = new UserRelationDto
            {
                Id = relation.Id,
                UserId = senderId,
                FriendId = receiverId,
                Status = RelationStatus.Rejected
            };

            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await relationBll.SyncUserRelation(dto, receiverId);

            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.UsersRelations.SingleAsync();
            persisted.Status.Should().Be(RelationStatus.Rejected);
        }

        [Fact]
        public async Task SyncUserRelation_WhenSenderTriesToAcceptOwnRequest_ReturnsFalse()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var relation = new UsersRelations
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = senderId,
                FriendId = receiverId,
                Status = RelationStatus.Pending
            };
            context.UsersRelations.Add(relation);
            await context.SaveChangesAsync();

            var relationBll = new UserRelationBll(context);
            var dto = new UserRelationDto
            {
                Id = relation.Id,
                UserId = senderId,
                FriendId = receiverId,
                Status = RelationStatus.Accepted
            };

            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await relationBll.SyncUserRelation(dto, senderId);

            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeFalse();
            var persisted = await context.UsersRelations.SingleAsync();
            persisted.Status.Should().Be(RelationStatus.Pending);
        }

        [Fact]
        public async Task SyncUserRelation_WhenThirdPartyTriesToModifyRelation_ReturnsFalse()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var attackerId = Guid.NewGuid();
            var relation = new UsersRelations
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = senderId,
                FriendId = receiverId,
                Status = RelationStatus.Pending
            };
            context.UsersRelations.Add(relation);
            await context.SaveChangesAsync();

            var relationBll = new UserRelationBll(context);
            var dto = new UserRelationDto
            {
                Id = relation.Id,
                UserId = senderId,
                FriendId = receiverId,
                Status = RelationStatus.Accepted
            };

            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await relationBll.SyncUserRelation(dto, attackerId);

            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeFalse();
            var persisted = await context.UsersRelations.SingleAsync();
            persisted.Status.Should().Be(RelationStatus.Pending);
            persisted.UserId.Should().Be(senderId);
            persisted.FriendId.Should().Be(receiverId);
        }

        [Fact]
        public async Task SyncUserRelation_WhenParticipantDeletesRelation_UsesServerTimestamp()
        {
            //---------------------------------------------------------------------------------------------------------------- Arrange
            await using var context = CreateInMemoryContext();
            var senderId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var relation = new UsersRelations
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = senderId,
                FriendId = receiverId,
                Status = RelationStatus.Accepted
            };
            context.UsersRelations.Add(relation);
            await context.SaveChangesAsync();

            var relationBll = new UserRelationBll(context);
            var dto = new UserRelationDto
            {
                Id = relation.Id,
                UserId = senderId,
                FriendId = receiverId,
                DeletedAt = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            //---------------------------------------------------------------------------------------------------------------- Act
            var result = await relationBll.SyncUserRelation(dto, senderId);

            //---------------------------------------------------------------------------------------------------------------- Assert
            result.Should().BeTrue();
            var persisted = await context.UsersRelations.SingleAsync();
            persisted.DeletedAt.Should().NotBeNull();
            persisted.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }
        #endregion
    }
}
