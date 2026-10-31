using HabityFit.DAL._1.Entity;
using Microsoft.EntityFrameworkCore;

namespace HabityFit.DAL._2.Context
{
    public class HabityFitDbContext : DbContext
    {
        #region Constructors
        // --------------------------------------------------------------------------------------------------- 
        public HabityFitDbContext(DbContextOptions<HabityFitDbContext> options) : base(options) { }
        // --------------------------------------------------------------------------------------------------- 
        #endregion


        #region Migration Zone
        // --------------------------------------------------------------------------------------------------- 
        //public HabityFitDbContext() { }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql("");
        //}
        // ---------------------------------------------------------------------------------------------------
        #endregion


        #region DbSets
        // --------------------------------------------------------------------------------------------------- Users, Relations and Invitations
        public DbSet<Users> Users { get; set; }
        public DbSet<UsersRelations> UsersRelations { get; set; }
        public DbSet<UsersInvitations> UsersInvitations { get; set; }
        // --------------------------------------------------------------------------------------------------- Body Measurements, Exercises, Routines, Workout Sessions
        public DbSet<BodyMeasurements> BodyMeasurements { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<MuscleGroup> MuscleGroups { get; set; }
        public DbSet<ExerciseMuscleGroup> ExerciseMuscleGroups { get; set; }
        public DbSet<Routines> Routines { get; set; }
        public DbSet<RoutineExercises> RoutineExercises { get; set; }
        public DbSet<RoutineRequests> RoutineRequests { get; set; }
        public DbSet<WorkoutSessions> WorkoutSessions { get; set; }
        public DbSet<WorkoutExercises> WorkoutExercises { get; set; }
        public DbSet<ExerciseSets> ExerciseSets { get; set; }
        // --------------------------------------------------------------------------------------------------- Feed Events, Logs
        public DbSet<FeedEvent> FeedEvents { get; set; }
        public DbSet<Logs> Logs { get; set; }
        // ---------------------------------------------------------------------------------------------------
        #endregion


        #region Configuration Indexes
        // ---------------------------------------------------------------------------------------------------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(BaseEntity.UpdatedAt));
                }
            }

            ConfigureIndexes(modelBuilder);
        }

        private static void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UsersInvitations>()
                .HasIndex(i => i.Token)
                .IsUnique();

            modelBuilder.Entity<Routines>()
                .HasIndex(r => new
                {
                    r.UserId,
                    r.UpdatedAt
                });

            modelBuilder.Entity<WorkoutSessions>()
                .HasIndex(ws => new
                {
                    ws.UserId,
                    ws.UpdatedAt
                });

            modelBuilder.Entity<BodyMeasurements>()
                .HasIndex(b => new
                {
                    b.IdUser,
                    b.UpdatedAt
                });

            modelBuilder.Entity<UsersRelations>()
                .HasIndex(ur => new
                {
                    ur.UserId,
                    ur.UpdatedAt
                });

            modelBuilder.Entity<UsersRelations>()
                .HasIndex(ur => new
                {
                    ur.FriendId,
                    ur.UpdatedAt
                });

            modelBuilder.Entity<RoutineRequests>()
                .HasIndex(rr => new
                {
                    rr.SenderId,
                    rr.UpdatedAt
                });

            modelBuilder.Entity<RoutineRequests>()
                .HasIndex(rr => new
                {
                    rr.ReceiverId,
                    rr.UpdatedAt
                });
        }
        // ---------------------------------------------------------------------------------------------------
        #endregion
    }
}
