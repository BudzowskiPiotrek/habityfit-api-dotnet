using HabityFit.API._2.Controllers.Common;
using HabityFit.API.Common;
using HabityFit.BLL._02.UsersManagement;
using HabityFit.BLL._02.UsersManagement.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HabityFit.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        #region Fields

        private readonly IUserBll _userBll;

        #endregion


        #region Constructors
        public UserController(IUserBll userBll, ILogger<UserController> logger) : base(logger)
        {
            _userBll = userBll;
        }

        #endregion


        #region Methods
        [EnableRateLimiting("SincronizacionLenta")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse
                {
                    WithError = true,
                    Message = "Email and password required",
                    StatusCode = 400
                });

            try
            {
                var user = await _userBll.Login(loginRequest);

                if (user == null)
                    return Unauthorized(new ApiResponse
                    {
                        WithError = true,
                        Message = "Invalid credentials",
                        StatusCode = 401
                    });

                return SuccessResponse(user, "Login successful.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized login attempt");
                return Unauthorized(new ApiResponse
                {
                    WithError = true,
                    Message = "Invalid credentials",
                    StatusCode = 401
                });
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex, "Login error");
            }
        }
        #endregion
    }
}
