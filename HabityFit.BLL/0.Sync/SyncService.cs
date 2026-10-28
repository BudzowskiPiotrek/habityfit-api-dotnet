using AutoMapper;
using HabityFit.BLL._0.Sync.Dto;
using HabityFit.BLL._02.UserRelationManagement;
using HabityFit.BLL._02.UserRelationManagement.Dto;
using HabityFit.BLL._02.UsersManagement;
using HabityFit.BLL._02.UsersManagement.Dto;
using HabityFit.BLL._02.UsersVerificationManagement;
using HabityFit.BLL._03.BodyMeasurementsManagement.Dto;
using HabityFit.BLL._04.ExercisesManagement;
using HabityFit.BLL._04.ExercisesManagement.Dto;
using HabityFit.BLL._05.MuscleGroupsManagement;
using HabityFit.BLL._05.MuscleGroupsManagement.Dto;
using HabityFit.BLL._06.ExerciseMuscleManagement;
using HabityFit.BLL._06.ExerciseMuscleManagement.Dto;
using HabityFit.BLL._07.RoutineRequestManagement;
using HabityFit.BLL._07.RoutineRequestManagement.Dto;
using HabityFit.BLL._07.RoutinesManagement;
using HabityFit.BLL._07.RoutinesManagement.Dto;
using HabityFit.BLL._08.RoutineExercisesManagement;
using HabityFit.BLL._08.RoutineExercisesManagement.Dto;
using HabityFit.BLL._09.WorkoutSessionsManagement;
using HabityFit.BLL._09.WorkoutSessionsManagement.Dto;
using HabityFit.BLL._1.Security;
using HabityFit.BLL._10.WorkoutExercisesManagement;
using HabityFit.BLL._10.WorkoutExercisesManagement.Dto;
using HabityFit.BLL._11.ExerciseSetsManagement;
using HabityFit.BLL._11.ExerciseSetsManagement.Dto;
using HabityFit.BLL._12.FeedEventManagement;
using HabityFit.BLL._12.FeedEventManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace HabityFit.BLL._0.Sync
{
    public class SyncService : ISyncService
    {
        #region Fields
        private readonly IBodyMeasurementsBll _bodyBll;
        private readonly IMuscleGroupBll _muscleGroupBll;
        private readonly IExercisesBll _exercisesBll;
        private readonly IExerciseMuscleGroupBll _exerciseMuscleBll;
        private readonly IRoutineBll _routineBll;
        private readonly IRoutineExercisesBll _routineExerciseBll;
        private readonly IWorkoutSessionsBll _workoutSessionBll;
        private readonly IWorkoutExercisesBll _workoutExerciseBll;
        private readonly IExerciseSetsBll _exerciseSetBll;
        private readonly IUserBll _userBll;
        private readonly IUserRelationBll _userRelationBll;
        private readonly IUsersVerificationBll _usersInvitationBll;
        private readonly IRoutineRequestBll _routineRequestBll;
        private readonly IFeedEventBll _feedEventBll;

        private readonly ITokenService _tokenService;
        private readonly ILogger<SyncService> _logger;
        private readonly IMapper _mapper;
        private readonly HabityFitDbContext _context;
        #endregion


        #region Constructors
        public SyncService(
            ITokenService tokenService,
            HabityFitDbContext context,
            IBodyMeasurementsBll bodyBll,
            IMuscleGroupBll muscleGroupBll,
            IExercisesBll exercisesBll,
            IExerciseMuscleGroupBll exerciseMuscleBll,
            IRoutineBll routineBll,
            IRoutineExercisesBll routineExerciseBll,
            IWorkoutSessionsBll workoutSessionBll,
            IWorkoutExercisesBll workoutExerciseBll,
            IExerciseSetsBll exerciseSetBll,
            IUserBll userBll,
            IUserRelationBll userRelationBll,
            IUsersVerificationBll usersInvitationBll,
            IRoutineRequestBll routineRequestBll,
            IFeedEventBll feedEventBll,
            IMapper mapper,
            ILogger<SyncService> logger)
        {
            _tokenService = tokenService;
            _context = context;
            _logger = logger;
            _bodyBll = bodyBll;
            _muscleGroupBll = muscleGroupBll;
            _exercisesBll = exercisesBll;
            _exerciseMuscleBll = exerciseMuscleBll;
            _routineBll = routineBll;
            _routineExerciseBll = routineExerciseBll;
            _workoutSessionBll = workoutSessionBll;
            _workoutExerciseBll = workoutExerciseBll;
            _exerciseSetBll = exerciseSetBll;
            _userBll = userBll;
            _userRelationBll = userRelationBll;
            _usersInvitationBll = usersInvitationBll;
            _routineRequestBll = routineRequestBll;
            _feedEventBll = feedEventBll;
            _mapper = mapper;
        }

        #endregion


        #region Methods
        public async Task<SyncErrorDetail> ProcessPush(SyncPayloadDto payload, Guid userId)
        {
            var response = new SyncErrorDetail
            {
                ItemId = new List<string>(),
                Success = new List<bool>()
            };

            // --- Processing of all collections ---
            await ProcessCollection(payload.Users, _userBll.SyncUser, x => x.Id.ToString(), response, x => x.Id = userId);
            await ProcessCollection(payload.BodyMeasurements, _bodyBll.SyncBody, x => x.Id.ToString(), response, x => x.IdUser = userId);
            await ProcessCollection(payload.Routines, _routineBll.SyncRoutine, x => x.Id.ToString(), response, x => x.UserId = userId);
            await ProcessCollection(payload.RoutineExercises, _routineExerciseBll.SyncRoutineExercise, x => x.Id.ToString(), response);
            await ProcessCollection(payload.WorkoutSessions, _workoutSessionBll.SyncWorkoutSession, x => x.Id.ToString(), response, x => x.UserId = userId);
            await ProcessCollection(payload.WorkoutExercises, _workoutExerciseBll.SyncWorkoutExercise, x => x.Id.ToString(), response);
            await ProcessCollection(payload.ExerciseSets, _exerciseSetBll.SyncExerciseSet, x => x.Id.ToString(), response);
            await ProcessCollection(payload.UserRelations, x => _userRelationBll.SyncUserRelation(x, userId), x => x.Id.ToString(), response);
            await ProcessCollection(payload.RoutinesRequests, _routineRequestBll.SyncRoutineRequest, x => x.Id.ToString(), response);
            await ProcessCollection(payload.UserInvitations, x => _usersInvitationBll.CreateAndSendInvitationAsync(x, userId), x => x.Id.ToString(), response);
            await ProcessCollection(payload.FeedEvents, _feedEventBll.SyncFeedEvent, x => x.Id.ToString(), response);

            // --- Logging ---
            await LogSyncResult(userId, response);

            return response;
        }

        public async Task<SyncPayloadDto?> ProcessPull(PullRequestDto request, Guid userId)
        {
            var response = new SyncPayloadDto();

            DateTime firstDateLogin = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            bool isFirstLogin = (request.LastSync <= firstDateLogin);

            /////////////////////////////////// EXERCISES
            var exerciseEntities = await _context.Exercises.AsNoTracking()
                .Where(e => e.UpdatedAt > request.LastSync).ToListAsync();

            response.Exercises = exerciseEntities
                .Select(e => _mapper.Map<ExercisesDto>(e)).ToList();

            /////////////////////////////////// MUSCLE GROUPS
            if (isFirstLogin)
            {

                var muscleEntities = await _context.MuscleGroups.AsNoTracking()
                    .Where(m => m.UpdatedAt > request.LastSync).ToListAsync();

                response.MuscleGroups = muscleEntities
                    .Select(m => _mapper.Map<MuscleGroupDto>(m)).ToList();
            }

            /////////////////////////////////// EXERCISE-MUSCLE
            if (isFirstLogin || exerciseEntities.Any())
            {

                var exerciseMuscleEntities = await _context.ExerciseMuscleGroups.AsNoTracking()
                    .Where(em => em.UpdatedAt > request.LastSync).ToListAsync();

                response.ExerciseMuscleGroups = exerciseMuscleEntities
                    .Select(em => _mapper.Map<ExerciseMuscleDto>(em)).ToList();
            }

            /////////////////////////////////// BODY
            var bodyEntities = await _context.BodyMeasurements.AsNoTracking()
                .Where(b => b.IdUser == request.UserId && b.UpdatedAt > request.LastSync).ToListAsync();

            response.BodyMeasurements = bodyEntities
                .Select(b => _mapper.Map<BodyMeasurementsDto>(b)).ToList();

            /////////////////////////////////// ROUTINES
            var routines = await _context.Routines.AsNoTracking()
                .Where(r => r.UserId == request.UserId && r.UpdatedAt > request.LastSync).ToListAsync();

            response.Routines = routines
                .Select(r => _mapper.Map<RoutineDto>(r)).ToList();

            /////////////////////////////////// ROUTINE EXERCISES
            var routineIds = routines.Select(r => r.Id).ToList();

            var routineExEntities = await _context.RoutineExercises.AsNoTracking()
                .Where(re => re.UpdatedAt > request.LastSync && routineIds.Contains(re.RoutineId)).ToListAsync();

            response.RoutineExercises = routineExEntities
                .Select(re => _mapper.Map<RoutineExerciseDto>(re)).ToList();

            /////////////////////////////////// WORKOUT SESSIONS
            var sessions = await _context.WorkoutSessions.AsNoTracking()
                .Where(ws => ws.UserId == request.UserId && ws.UpdatedAt > request.LastSync).ToListAsync();

            response.WorkoutSessions = sessions
                .Select(ws => _mapper.Map<WorkoutSessionsDto>(ws)).ToList();

            /////////////////////////////////// WORKOUT EXERCISES
            var sessionIds = sessions.Select(ws => ws.Id).ToList();

            var workoutExEntities = await _context.WorkoutExercises.AsNoTracking()
                .Where(we => we.UpdatedAt > request.LastSync && sessionIds.Contains(we.SessionId)).ToListAsync();

            response.WorkoutExercises = workoutExEntities
                .Select(we => _mapper.Map<WorkoutExercisesDto>(we)).ToList();

            /////////////////////////////////// SETS
            var workoutExIds = workoutExEntities.Select(we => we.Id).ToList();

            var setEntities = await _context.ExerciseSets.AsNoTracking()
                .Where(es => es.UpdatedAt > request.LastSync && workoutExIds.Contains(es.WorkoutExerciseId)).ToListAsync();

            response.ExerciseSets = setEntities
                .Select(es => _mapper.Map<ExerciseSetsDto>(es)).ToList();

            /////////////////////////////////// ALL USER RELATIONS
            var allUserRelations = await _context.UsersRelations.AsNoTracking()
                .Where(ur => ur.UserId == request.UserId || ur.FriendId == request.UserId)
                .ToListAsync();

            var allAcceptedFriendIds = allUserRelations
                .Where(ur => ur.Status == RelationStatus.Accepted)
                .Select(ur => ur.UserId == request.UserId ? ur.FriendId : ur.UserId).ToList();

            /////////////////////////////////// USER RELATIONS
            var relationEntities = allUserRelations
                .Where(ur => ur.UpdatedAt > request.LastSync).ToList();

            response.UserRelations = relationEntities
                .Select(ur => _mapper.Map<UserRelationDto>(ur)).ToList();

            /////////////////////////////////// USERS (SELF + FRIENDS)
            var targetUserIds = new HashSet<Guid>();

            // ADD = If the user updated themselves, if not, nothing at all!
            var selfUpdated = await _context.Users.AsNoTracking()
                .AnyAsync(u => u.Id == request.UserId && u.UpdatedAt > request.LastSync);

            if (selfUpdated) targetUserIds.Add(request.UserId);

            // ADD = The whole list of new contacts pending/accepted
            foreach (var ur in relationEntities)
                if (ur.Status != RelationStatus.Rejected)
                    targetUserIds.Add(ur.UserId == request.UserId ? ur.FriendId : ur.UserId);

            #region UPDATED FRIENDS (ONLY IF USER UPDATED THEIR PROFILE)
            //var updatedFriendIds = await context.UsersRelations
            //    .Where(ur => (ur.UserId == request.UserId || ur.FriendId == request.UserId) && ur.Status != RelationStatus.Rejected)
            //    .Select(ur => ur.UserId == request.UserId ? ur.FriendId : ur.UserId)
            //    .Distinct()
            //    .Join(context.Users.Where(u => u.UpdatedAt > request.LastSync), friendId => friendId, user => user.Id, (friendId, user) => friendId).ToListAsync();

            //// ADD = All updated profiles from our contacts
            //foreach (var id in updatedFriendIds)
            //    targetUserIds.Add(id);
            #endregion

            var userEntities = await _context.Users.AsNoTracking()
                .Where(u => targetUserIds.Contains(u.Id)).ToListAsync();

            response.Users = userEntities
                .Select(u => _mapper.Map<UserReadDto>(u)).ToList();

            /////////////////////////////////// ROUTINE REQUESTS
            var routineReqEntities = await _context.RoutineRequests.AsNoTracking()
                .Where(rr => (rr.SenderId == request.UserId || rr.ReceiverId == request.UserId)
                             && rr.UpdatedAt > request.LastSync).ToListAsync();

            response.RoutinesRequests = routineReqEntities
                .Select(rr => _mapper.Map<RoutineRequestDto>(rr)).ToList();

            ////////////////////////////////// FEED EVENT
            var minFeedDate = request.LastSync > DateTime.UtcNow.AddDays(-10) ? request.LastSync : DateTime.UtcNow.AddDays(-10);

            var feedEvent = await _context.FeedEvents.AsNoTracking()
                .Where(fe => (fe.UserId == null || allAcceptedFriendIds.Contains(fe.UserId.Value))
                             && fe.UpdatedAt > minFeedDate).ToListAsync();

            response.FeedEvents = feedEvent
                .Select(fe => _mapper.Map<FeedEventDto>(fe)).ToList();

            ////////////////////////////////// TOKEN GENERATION 
            var userReadDto = await _userBll.GetUserById(userId);
            if (userReadDto != null)
            {
                response.Token = _tokenService.CreateToken(userReadDto);
            }

            return response;
        }
        #endregion


        #region Private Helpers
        private async Task ProcessCollection<T>(
            List<T>? collection, Func<T, Task<bool>> syncFunc,
            Func<T, string> idSelector, SyncErrorDetail response,
            Action<T>? setUserId = null)
        {
            if (collection == null || !collection.Any()) return;

            foreach (var item in collection)
            {
                string id = idSelector(item);
                try
                {
                    setUserId?.Invoke(item);

                    bool result = await syncFunc(item);

                    response.ItemId.Add(id);
                    response.Success.Add(result);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to synchronize item {ItemId} of the type {EntityType}", id, typeof(T).Name);
                    response.ItemId.Add(id);
                    response.Success.Add(false);
                }
            }
        }

        private async Task LogSyncResult(Guid userId, SyncErrorDetail response)
        {
            int total = response.ItemId.Count;
            if (total == 0) return;

            int successful = response.Success.Count(s => s);
            int failed = total - successful;

            var log = new Logs
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                Type = $"[INFO] [SYNC] {total} items processed | Success: {successful} | Fail: {failed}"
            };

            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}

