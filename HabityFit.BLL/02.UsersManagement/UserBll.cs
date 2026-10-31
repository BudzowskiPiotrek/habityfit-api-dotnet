
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using HabityFit.BLL._0.Common;
using HabityFit.BLL._02.UsersManagement.Dto;
using HabityFit.BLL._1.Security;
using HabityFit.DAL._1.Entity;
using HabityFit.DAL._2.Context;

namespace HabityFit.BLL._02.UsersManagement
{
    public class UserBll : IUserBll
    {
        #region Fields
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly HabityFitDbContext _context;
        #endregion


        #region Constructors
        public UserBll(HabityFitDbContext context, IAuthService authService, IMapper mapper, ITokenService tokenService, IPasswordService passwordService)
        {
            _context = context;
            _authService = authService;
            _mapper = mapper;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }
        #endregion


        #region Methods
        public Task<LoginResponseDto?> Login(LoginRequestDto loginRequest)
        {
            return _authService.AuthenticateAsync(loginRequest.Email, loginRequest.Password);
        }

        public async Task<bool> SyncUser(UserReadDto dto)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (existing == null)
            {
                return false;
            }

            if (existing.DeletedAt != null && dto.DeletedAt == null)
            {
                existing.DeletedAt = null;
            }

            if (dto.UpdatedAt > (existing.UpdatedAt ?? DateTime.MinValue))
            {
                existing.BirthDate = dto.BirthDate;
                existing.FullName = dto.FullName;
                existing.Sex = dto.Sex;
                existing.Height = dto.Height;
                existing.TermsAcceptedAt = dto.TermsAcceptedAt;
                existing.UpdatedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync() > 0;
            }
            return true;
        }

        public async Task<UserReadDto?> GetUserById(Guid userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

            return user == null ? null : _mapper.Map<UserReadDto>(user);
        }

        public async Task<LoginResponseDto?> CreateUserAsync(UserDto user)
        {
            var normalizedEmail = EmailNormalizer.Normalize(user.Email);
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (existingUser != null) throw new InvalidOperationException("User with this email already exists.");

            var newUser = new Users
            {
                Id = Guid.NewGuid(),
                FullName = user.FullName,
                Email = normalizedEmail,
                PasswordHash = _passwordService.HashPassword(user.Password),
                CreatedAt = DateTime.UtcNow,
            };
            _context.Users.Add(newUser);

            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = _tokenService.CreateToken(_mapper.Map<UserReadDto>(newUser)),
                User = _mapper.Map<UserReadDto>(newUser),
            };
        }
        #endregion
    }
}
