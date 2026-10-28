using AutoMapper;
using HabityFit.BLL._02.UserRelationManagement.Dto;
using HabityFit.BLL._02.UsersManagement;
using HabityFit.BLL._02.UsersManagement.Dto;
using HabityFit.BLL._02.UsersVerificationManagement.Dto;
using HabityFit.BLL._03.BodyMeasurementsManagement.Dto;
using HabityFit.BLL._04.ExercisesManagement.Dto;
using HabityFit.BLL._05.MuscleGroupsManagement.Dto;
using HabityFit.BLL._06.ExerciseMuscleManagement.Dto;
using HabityFit.BLL._07.RoutineRequestManagement.Dto;
using HabityFit.BLL._07.RoutinesManagement.Dto;
using HabityFit.BLL._08.RoutineExercisesManagement.Dto;
using HabityFit.BLL._09.WorkoutSessionsManagement.Dto;
using HabityFit.BLL._10.WorkoutExercisesManagement.Dto;
using HabityFit.BLL._11.ExerciseSetsManagement.Dto;
using HabityFit.BLL._12.FeedEventManagement.Dto;
using HabityFit.DAL._1.Entity;

namespace HabityFit.BLL._0.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Users, UserReadDto>().ReverseMap();
            CreateMap<UserDto, Users>();
            CreateMap<UsersRelations, UserRelationDto>();
            CreateMap<UsersInvitations, UsersInvitationDto>().ReverseMap();
            CreateMap<BodyMeasurements, BodyMeasurementsDto>().ReverseMap();
            CreateMap<Exercise, ExercisesDto>().ReverseMap();
            CreateMap<MuscleGroup, MuscleGroupDto>().ReverseMap();
            CreateMap<ExerciseMuscleGroup, ExerciseMuscleDto>().ReverseMap();
            CreateMap<Routines, RoutineDto>().ReverseMap();
            CreateMap<RoutineRequests, RoutineRequestDto>().ReverseMap();
            CreateMap<RoutineExercises, RoutineExerciseDto>().ReverseMap();
            CreateMap<WorkoutSessions, WorkoutSessionsDto>().ReverseMap();
            CreateMap<WorkoutExercises, WorkoutExercisesDto>().ReverseMap();
            CreateMap<ExerciseSets, ExerciseSetsDto>().ReverseMap();
            CreateMap<FeedEvent, FeedEventDto>().ReverseMap();
        }
    }
}
