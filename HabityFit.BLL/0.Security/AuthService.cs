using AutoMapper;
using HabityFit.BLL._02.UsersManagement.Dto;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HabityFit.BLL._1.Security
{
    public class AuthService : IAuthService
    {
        #region Fields
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly HabityFitDbContext _context;
        private readonly ILogger<AuthService> _logger;
        #endregion


        #region Constructors
        public AuthService(
            IPasswordService passwordService,
            ITokenService tokenService,
            HabityFitDbContext context,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _passwordService = passwordService;
            _tokenService = tokenService;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        #endregion


        #region Methods
        public async Task<LoginResponseDto?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new UnauthorizedAccessException("Email and password must be provided.");
            var normalizedEmail = email.Trim().ToLower();
            // Verify user credentials
            var log = new Logs
            {
                Id = Guid.NewGuid(),
                UserId = null,
                CreatedAt = DateTime.UtcNow,
            };

            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.DeletedAt == null);

            if (userEntity != null)
            {
                var lastMinute = DateTime.UtcNow.AddMinutes(-1);
                var failedAttempts = await _context.Logs.CountAsync(l => l.UserId == userEntity.Id &&
                                                                        l.Type == "[INFO] [AUTH] Access Failed - Invalid Password" &&
                                                                        l.CreatedAt > lastMinute);

                if (failedAttempts >= 3)
                {
                    _logger.LogWarning("Login blocked due to failed attempts. UserId: {UserId}", userEntity.Id);
                    log.UserId = userEntity.Id;
                    log.Type = "[WARN] [AUTH] Login Blocked - Too many attempts";
                    await _context.Logs.AddAsync(log);
                    await _context.SaveChangesAsync();
                    throw new UnauthorizedAccessException("Too many atempts. Account locked for 1 min.");
                }
            }
            else
            {
                log.Type = "[INFO] [AUTH] Access Failed - User Not Found";
                await _context.Logs.AddAsync(log);
                await _context.SaveChangesAsync();
                throw new UnauthorizedAccessException("User not found");

            }
            log.UserId = userEntity.Id;
            // Verify password
            bool isValid = _passwordService.VerifyPassword(userEntity.PasswordHash, password, out bool needRehash);

            if (!isValid)
            {
                log.Type = "[INFO] [AUTH] Access Failed - Invalid Password";
                await _context.Logs.AddAsync(log);
                await _context.SaveChangesAsync();
                throw new UnauthorizedAccessException("Invalid password");
            }

            if (needRehash)
            {
                userEntity.PasswordHash = _passwordService.HashPassword(password);
                _context.Users.Update(userEntity);
                await _context.SaveChangesAsync();
            }
            // Map user entity to DTO
            var userReadDto = _mapper.Map<UserReadDto>(userEntity);

            // Generate JWT token
            var token = _tokenService.CreateToken(userReadDto);

            // Log the login event
            log.UserId = userEntity.Id;
            log.Type = "[INFO] [AUTH] Access Success";
            await _context.Logs.AddAsync(log);
            await _context.SaveChangesAsync();

            var result = new LoginResponseDto
            {
                Token = token,
                User = userReadDto,
                Expiration = DateTime.UtcNow.AddDays(7)
            };

            return result;
        }
        #endregion
    }
}
