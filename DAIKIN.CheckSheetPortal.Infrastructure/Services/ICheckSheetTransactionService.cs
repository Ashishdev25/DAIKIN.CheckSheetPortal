using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using System.Security.Claims;

namespace DAIKIN.CheckSheetPortal.Infrastructure.Services
{
    public interface ICheckSheetTransactionService : IBaseService<CheckSheetTransaction>
    {
        Task<IOperationResponse> GetTransactionsAsync(string globalSearch, bool searchByDate, DateTime fromDate, DateTime toDate);
        Task<(MemoryStream, string)> ExportTransactionsAsync(bool exportWithCheckPoints, string globalSearch, bool searchByDate, DateTime fromDate, DateTime toDate);
        Task<IOperationResponse> GetCheckSheetsAsync(string globalSearch, UserDTO user);
        Task<IOperationResponse> GetCheckSheetByIdAsync(string id, UserDTO user);
        Task<IOperationResponse> BulkUpdateCheckRecordAsync(CheckPointBulkEntryDTO checkPointBulkEntryDTO, UserDTO user);
        Task<IOperationResponse> UpdateCheckRecordAsync(CheckPointEntryDTO checkPointEntryDTO, UserDTO user);
        Task<(MemoryStream, string)> ExportCheckSheetByIdAsync(string id);
        Task<IOperationResponse> SubmitCheckSheetAsync(string checkSheetId, UserDTO user);
        Task<IOperationResponse> ApproveCheckSheetAsync(string checkSheetId, UserDTO user);
        Task<IOperationResponse> SendCheckSheetEmails(string api_key);
        Task<IOperationResponse> ArchiveAsync(string api_key);
        Task<IOperationResponse> ArchiveInitialRunAsync(string api_key);
    }
}
