using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Net;
using HabityFit.BLL._0.Common;
using HabityFit.BLL._0.Infrastructure;
using HabityFit.BLL._0.Sync;
using HabityFit.BLL._02.UserRelationManagement;
using HabityFit.BLL._02.UsersManagement;
using HabityFit.BLL._02.UsersVerificationManagement;
using HabityFit.BLL._03.BodyMeasurementsManagement;
using HabityFit.BLL._03.BodyMeasurementsManagement.Dto;
using HabityFit.BLL._04.ExercisesManagement;
using HabityFit.BLL._05.MuscleGroupsManagement;
using HabityFit.BLL._06.ExerciseMuscleManagement;
using HabityFit.BLL._07.RoutineRequestManagement;
using HabityFit.BLL._07.RoutinesManagement;
using HabityFit.BLL._08.RoutineExercisesManagement;
using HabityFit.BLL._09.WorkoutSessionsManagement;
using HabityFit.BLL._1.Security;
using HabityFit.BLL._10.WorkoutExercisesManagement;
using HabityFit.BLL._11.ExerciseSetsManagement;
using HabityFit.BLL._12.FeedEventManagement;
using HabityFit.DAL._2.Context;
using System.Text;
using System.Threading.RateLimiting;
//---------------------------------------------------------------------------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);
//------------------------------------------------------------------------------------------ Configure Serilog logging ------------------------
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration));
//------------------------------------------------------------------------------------------ Add services to the container. -------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//------------------------------------------------------------------------------------------ Add CORS policy ----------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Allows any origin (such as your mobile IP address)
      .AllowAnyMethod()           // Allows all HTTP methods (GET, POST, PUT, DELETE, etc.)
      .AllowAnyHeader();          // Allows any request header (tokens, content-type, etc.)
    });

    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://setpoint.info")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
//------------------------------------------------------------------------------------------ Configure Database Context ----------------------
builder.Services.AddDbContext<HabityFitDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnection")));
//------------------------------------------------------------------------------------------ Configure AutoMapper -----------------------------
builder.Services.AddSingleton(provider => new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new MappingProfile());
}).CreateMapper());
//------------------------------------------------------------------------------------------ Dependency Injection for BLL and Services --------
builder.Services.AddScoped<IUserBll, UserBll>();
builder.Services.AddScoped<IRoutineBll, RoutineBll>();
builder.Services.AddScoped<IFeedEventBll, FeedEventBll>();
builder.Services.AddScoped<IExercisesBll, ExercisesBll>();
builder.Services.AddScoped<IMuscleGroupBll, MuscleGroupBll>();
builder.Services.AddScoped<IExerciseSetsBll, ExerciseSetsBll>();
builder.Services.AddScoped<IUserRelationBll, UserRelationBll>();
builder.Services.AddScoped<IRoutineRequestBll, RoutineRequestBll>();
builder.Services.AddScoped<IUsersVerificationBll, UsersVerificationBll>();
builder.Services.AddScoped<IWorkoutSessionsBll, WorkoutSessionsBll>();
builder.Services.AddScoped<IRoutineExercisesBll, RoutineExercisesBll>();
builder.Services.AddScoped<IBodyMeasurementsBll, BodyMeasurementsBll>();
builder.Services.AddScoped<IWorkoutExercisesBll, WorkoutExercisesBll>();
builder.Services.AddScoped<IExerciseMuscleGroupBll, ExerciseMuscleGroupBll>();
//------------------------------------------------------------------------------------------ Dependency Injection Services --------------------
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
//------------------------------------------------------------------------------------------ Configure JWT Authentication ---------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
    };
});
//------------------------------------------------------------------------------------------ Configure Forwarded Headers for Reverse Proxy ----
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;

    options.ForwardLimit = 1;
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
});
//------------------------------------------------------------------------------------------ Configure Rate Limiting --------------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("SincronizacionLenta", context =>
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: remoteIp,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
    options.AddPolicy("Register", context =>
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: remoteIp,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromHours(24),
                QueueLimit = 0
            });
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
//---------------------------------------------------------------------------------------------------------------------------------------------
var app = builder.Build();
app.UseForwardedHeaders();
//------------------------------------------------------------------------------------------- Configure Serilog request logging and global exception handling 
app.UseSerilogRequestLogging();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var ex = exceptionHandlerFeature?.Error;

        Log.Error(ex, "Unhandled exception in {Path}", exceptionHandlerFeature?.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new HabityFit.API.Common.ApiResponse
        {
            WithError = true,
            Message = "Internal server error",
            StatusCode = 500
        });
    });
});
//------------------------------------------------------------------------------------------ Configure the HTTP request pipeline. -------------
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseCors("ProductionPolicy");
    app.UseHttpsRedirection();
}
//---------------------------------------------------------------------------------------------------------------------------------------------
app.UseRouting();        // Ensures routing executes before authentication and authorization
app.UseRateLimiter();    // Ensures rate limiting executes before authorization to protect endpoints
app.UseAuthentication(); // Ensures authentication executes before authorization
app.UseAuthorization();  // Ensures authorization executes after authentication and rate limiting
app.MapControllers();    // Ensures controller endpoints are mapped and available for routing
app.Run();
