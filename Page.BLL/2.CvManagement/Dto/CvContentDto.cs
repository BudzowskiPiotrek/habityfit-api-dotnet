namespace Page.BLL._2.CvManagement.Dto
{
    public class CvContentDto
    {
        public CvProfileDto Profile { get; set; } = new();
        public List<CvExperienceDto> Experience { get; set; } = [];
    }
}
