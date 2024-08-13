using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;

namespace DAIKIN.CheckSheetPortal.Infrastructure.Services
{
    public interface ICheckSheetVersionService : IBaseService<CheckSheetVersion>
    {
        Task<IOperationResponse> GetAllAsync(string globalSearch);
        Task<IOperationResponse> GetLatestVersionsAsync(string globalSearch);
        Task<IOperationResponse> CreateAsync(CheckSheetCreateDTO checkSheetCreateDTO, UserDTO user);
        Task<IOperationResponse> CreateVersionAsync(CheckSheetCreateVersionDTO checkSheetCreateVersionDTO, UserDTO user);
        Task<IOperationResponse> CreateCheckPointAsync(string checkSheetId, CheckPointDTO checkPointDTO);
        Task<IOperationResponse> UpdateWorkFlowAsync(CheckSheetCreateVersionDTO checkSheetCreateVersionDTO, UserDTO user);
        Task<IOperationResponse> UpdateCheckPointAsync(string checkSheetId, CheckPointDTO checkPointDTO);
        Task<CheckPoint> UpdateCheckSheetImageAsync(string checkSheetId, string checkPointId, string uniqueFileName, string fileName);
        Task<IOperationResponse> GetCheckSheetByIdAsync(string checkSheetId, UserDTO user);
        Task<IOperationResponse> ReplicateAsync(CheckSheetReplicateDTO checkSheetReplicateDTO, UserDTO user);
        Task<IOperationResponse> UpdateCheckSheetAsync(string checkSheetId, CheckSheetVersionUpdateDTO checkSheetVersionUpdate, UserDTO user);
        Task<IOperationResponse> GetReviewersAndApproversAsync();
        Task<IOperationResponse> SubmitCheckSheetAsync(string checkSheetId, string changeDetails, UserDTO user);
        Task<IOperationResponse> ReviewCheckSheetAsync(string checkSheetId, UserDTO user);
        Task<IOperationResponse> ApproveCheckSheetAsync(string checkSheetId, UserDTO user);
        Task<IOperationResponse> RejectCheckSheetAsync(string checkSheetId, string reason, UserDTO user);
        Task<IOperationResponse> MoveApprovedCheckSheets(string api_key);
        Task<IOperationResponse> DeleteCheckPointAsync(string checkSheetId, string id);
    }
}
