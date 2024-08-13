using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;

namespace DAIKIN.CheckSheetPortal.Infrastructure.Services
{
    public interface IConfigurationService : IBaseService<Configuration>
    {
        Task<IOperationResponse> DeleteAsync(string id, UserDTO user);
        Task<IOperationResponse> CreateAsync(Configuration model, UserDTO user);
        Task<IOperationResponse> UpdateAsync(string id, Configuration model, UserDTO user);
    }
}
