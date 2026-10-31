using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HabityFit.API._2.Controllers.Common;
using HabityFit.API.Common;
using HabityFit.BLL._0.Sync;
using HabityFit.BLL._0.Sync.Dto;

namespace HabityFit.API.Controllers.SyncController
{
    [Authorize]
    [EnableRateLimiting("SincronizacionLenta")]
    [Route("api/[controller]")]
    [ApiController]
    public class SyncController : BaseController
    {
        #region Fields

        private readonly ISyncService _syncBll;

        #endregion

        #region Constructors

        public SyncController(ISyncService syncBll, ILogger<SyncController> logger) : base(logger)
        {
            _syncBll = syncBll;
        }

        #endregion

        #region Methods

        [HttpPost("push")]
        [RequestSizeLimit(300_000)]
        public async Task<IActionResult> Push([FromBody] SyncPayloadDto payload)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse
                {
                    WithError = true,
                    Message = "Invalid sync data structure",
                    StatusCode = 400
                });

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new ApiResponse
                {
                    WithError = true,
                    Message = "User ID not found in token",
                    StatusCode = 401
                });

            try
            {

                var result = await _syncBll.ProcessPush(payload, userId);


                return SuccessResponse(result, "Data synchronized successfully.", 200);
            }
            catch (Exception ex)
            {
                var detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return StatusCode(500, new ApiResponse
                {
                    WithError = true,
                    Message = $"Push error",
                    StatusCode = 500
                });
            }
        }

        [HttpPost("pull")]
        public async Task<IActionResult> Pull([FromBody] PullRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse
                {
                    Message = "Invalid request",
                    StatusCode = 400
                });

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized(new ApiResponse
                {
                    WithError = true,
                    Message = "User ID not found in token",
                    StatusCode = 401
                });

            request.UserId = userId;

            try
            {
                var result = await _syncBll.ProcessPull(request, userId);

                return SuccessResponse(result, "Delta sync completed.", 200);
            }
            catch (Exception ex)
            {
                var detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return StatusCode(500, new ApiResponse
                {
                    WithError = true,
                    Message = $"Pull error",
                    StatusCode = 500
                });
            }
        }

        #endregion
    }
}
