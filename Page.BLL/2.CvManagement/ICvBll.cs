using Page.BLL._2.CvManagement.Dto;

namespace Page.BLL._2.CvManagement
{
    public interface ICvBll
    {
        Task<CvContentDto?> GetCv();
    }
}
