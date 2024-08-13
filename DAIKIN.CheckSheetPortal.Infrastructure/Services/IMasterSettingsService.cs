using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;

namespace DAIKIN.CheckSheetPortal.Infrastructure.Services
{
    public interface IMasterSettingsService : IBaseService<MasterSettings>
    {
        Task<IOperationResponse> GetSettingsAsync();
        Task<IOperationResponse> UpdateAsync(string id, MasterSettingsDTO model, UserDTO user);
    }
}
