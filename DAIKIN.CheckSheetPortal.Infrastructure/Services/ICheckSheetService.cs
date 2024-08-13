using DAIKIN.CheckSheetPortal.Entities;

namespace DAIKIN.CheckSheetPortal.Infrastructure.Services
{
    public interface ICheckSheetService : IBaseService<CheckSheet>
    {
        Task<IOperationResponse> GenerateDailyCheckSheetsAsync(string api_key);
    }
}
