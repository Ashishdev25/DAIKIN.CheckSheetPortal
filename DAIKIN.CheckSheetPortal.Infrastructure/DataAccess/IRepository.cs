using DAIKIN.CheckSheetPortal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.Infrastructure
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetByFilterAsync(string column, object value);
        Task<IEnumerable<T>> GetByFilterAsync(Dictionary<string, object> filters);
        Task<IEnumerable<T>> GetByBetweenDatesAsync(string column, DateTime fromDate, DateTime toDate);
        Task<bool> GetByFilterAnyAsync(string column, object value);
        Task<T> FirstOrDefaultAsync(string column, object value);
        Task<T> CreateAsync(T document);
        Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> documents);
        Task InsertBulkAsync(IEnumerable<T> documents);
        void InsertBulk(IEnumerable<T> documents);
        Task UpdateAsync(string id, T document);
        Task DeleteAsync(string id);
        Task DeleteManyAsync(string column, object value);
        Task DropTableAsync(string tablename);
        Task<IEnumerable<T>> GetByFilterInAsync(string column, IEnumerable<object> values);
        Task<int> GetRecordCountAsync();
        Task<(IEnumerable<T>, long)> GetAllWithPaginationAsync(DynamicTable dynamicTable);
        Task DeleteManyValuesAsync(string column, List<string> values);
    }
}
