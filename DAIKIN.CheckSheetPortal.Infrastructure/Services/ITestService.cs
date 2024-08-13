using DAIKIN.CheckSheetPortal.Entities;

namespace DAIKIN.CheckSheetPortal.Infrastructure.Services
{
    public interface ITestService
    {
        bool SendMail(string subject, string body, string env, string emailTo, string emailCC);
    }
}
