using HabityFit.API._2.Controllers.Common;
using HabityFit.API.Common;
using HabityFit.BLL._02.UsersVerificationManagement;
using HabityFit.BLL._02.UsersVerificationManagement.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.RegularExpressions;

namespace HabityFit.API.Controllers.InvitationsControler
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationsController : BaseController
    {
        #region Fields
        private readonly IUsersVerificationBll _invitationBll;
        private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        #endregion


        #region Constructors
        public InvitationsController(IUsersVerificationBll invitationBll, ILogger<InvitationsController> logger) : base(logger)
        {
            _invitationBll = invitationBll;
        }
        #endregion


        #region Methods
        [EnableRateLimiting("Register")]
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] EmailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || !EmailRegex.IsMatch(dto.Email))
            {
                return BadRequest(new ApiResponse
                {
                    WithError = true,
                    Message = "Invalid email format",
                    StatusCode = 400
                });
            }

            try
            {
                var sent = await _invitationBll.CreateAndSendValidateAsync(dto.Email);

                if (!sent)
                    return StatusCode(500, ApiResponse.Error("Error sending validation email", 500));

                return SuccessResponse(null, "Validation email sent.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation on register");
                return Conflict(new ApiResponse
                {
                    WithError = true,
                    Message = "Request could not be processed",
                    StatusCode = 409
                });
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex, "Error processing register");
            }
        }

        [EnableRateLimiting("SincronizacionLenta")]
        [HttpPost("accept")]
        [AllowAnonymous]
        public async Task<IActionResult> Accept([FromBody] AcceptInvitationDto dto)
        {
            try
            {
                var result = await _invitationBll.AcceptInvitationAsync(dto.Token, dto.FullName, dto.Password);

                if (result == null)
                    return NotFound(new ApiResponse
                    {
                        WithError = true,
                        Message = "Invalid or expired invitation",
                        StatusCode = 404
                    });

                return SuccessResponse(result, "Account created successfully.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation on invitation accept");
                return Conflict(new ApiResponse
                {
                    WithError = true,
                    Message = "Request could not be processed",
                    StatusCode = 409
                });
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex, "Error processing invitation");
            }
        }

        [EnableRateLimiting("SincronizacionLenta")]
        [HttpGet("activate")]
        [AllowAnonymous]
        public IActionResult Activate([FromQuery] string token)
        {
            return Redirect($"habityfit://activate?token={token}");
        }
        #endregion
    }
}
