namespace HabityFit.BLL._0.Common
{
    public static class ValidationLimits
    {
        public static class User
        {
            public const int EmailMaxLength = 254;
            public const int FullNameMinLength = 2;
            public const int FullNameMaxLength = 100;
        }

        public static class Password
        {
            public const int MinLength = 12;
            public const int MaxLength = 128;
        }

        public static class Feed
        {
            public const int CustomTextMaxLength = 500;
        }

        public static class Exercise
        {
            public const int NameMaxLength = 100;
            public const int DescriptionMaxLength = 1000;
            public const int ImageUrlMaxLength = 2048;
        }

        public static class Routine
        {
            public const int NameMaxLength = 100;
            public const int DescriptionMaxLength = 1000;
        }

        public static class MuscleGroup
        {
            public const int NameMaxLength = 100;
            public const int DescriptionMaxLength = 1000;
        }

        public static class WorkoutSession
        {
            public const int NotesMaxLength = 1000;
        }
    }
}
