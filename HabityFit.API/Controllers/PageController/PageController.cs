using HabityFit.API._2.Controllers.Common;
using HabityFit.API.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Page.BLL._1.VideosManagement;
using Page.DAL._1.Entity;

namespace HabityFit.API.Controllers
{
    [EnableRateLimiting("PagePublic")]
    [Route("api/[controller]")]
    [ApiController]
    public class PageController : BaseController
    {
        #region Fields
        private readonly IVideosBll _videosBll;
        #endregion


        #region Constructors
        public PageController(IVideosBll videosBll, ILogger<PageController> logger) : base(logger)
        {
            _videosBll = videosBll;
        }
        #endregion


        #region Methods
        [HttpGet("videos/{section:int}")]
        public async Task<IActionResult> GetVideos(int section)
        {
            if (!Enum.IsDefined(typeof(SectionType), section))
                return BadRequest(ApiResponse.Error("Invalid section", 400));

            try
            {
                var content = await _videosBll.GetVideos((SectionType)section);

                if (content is null)
                    return NotFound(ApiResponse.Error("Section not found", 404));

                return SuccessResponse(content, "Videos retrieved.");
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex, "Error retrieving videos");
            }
        }
        #endregion
    }
}
