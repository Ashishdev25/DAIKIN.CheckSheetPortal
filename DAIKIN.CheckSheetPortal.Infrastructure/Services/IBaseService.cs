using DAIKIN.CheckSheetPortal.Entities;

namespace DAIKIN.CheckSheetPortal.Infrastructure
{
    public interface IBaseService<T> where T : class
    {
        Task<IOperationResponse> GetAllAsync();
        Task<IOperationResponse> GetByIdAsync(string id);
        Task<IOperationResponse> GetByFilterAsync(string column, string id);
        Task<IOperationResponse> DeleteAsync(string id);
        Task<IOperationResponse> CreateAsync(T model);
        Task<IOperationResponse> UpdateAsync(string id, T model);
        Task<IOperationResponse> CreateBulkAsync(IEnumerable<T> models);
        Task<IOperationResponse> GetAllWithPaginationAsync(DynamicTable dynamicTable);
    }
}