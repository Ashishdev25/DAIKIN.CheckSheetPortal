using AutoMapper;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace DAIKIN.CheckSheetPortal.Services
{
    public abstract class BaseService<T> : IBaseService<T> where T : class
    {
        private readonly IRepository<T> _repository;
        protected BaseService(IRepository<T> repository, IMapper mapper)
        {
            _repository = repository;
        }
        public virtual async Task<IOperationResponse> CreateAsync(T model)
        {
            ValidationContext context = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                if (model.GetType().Name == "User")
                {
                    var items = await _repository.GetAllAsync();
                    var itemexists = ((List<User>)items).Where(x => x.Email == model.GetType().GetProperty("Email").GetValue(model).ToString());
                    if (itemexists.Count() > 0) return OperationResponse.Error(Common.Util.UserEmailAlreadyExists);
                }
                else if (model.GetType().Name == "Role")
                {
                    var items = await _repository.GetAllAsync();
                    var itemexists = ((List<Role>)items).Where(x => x.Name == model.GetType().GetProperty("Name").GetValue(model).ToString());
                    if (itemexists.Count() > 0) return OperationResponse.Error(Common.Util.RoleNameAlreadyExists);
                }
                var item = await _repository.CreateAsync(model);
                return OperationResponse<T>.Success(item, $"{model.GetType().Name} added successfully");
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public virtual async Task<IOperationResponse> UpdateAsync(string id, T model)
        {
            ValidationContext context = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                var canDeleteProperty = model.GetType().GetProperty("CanDelete");
                if (canDeleteProperty != null && !(bool)canDeleteProperty.GetValue(model))
                {
                    return OperationResponse.Error($"Sorry! The default {model.GetType().Name} cannot be updated");
                }

                if (model.GetType().Name == "User")
                {
                    var items = await _repository.GetAllAsync();
                    var itemexists = ((List<User>)items).Where(x => x.Email == model.GetType().GetProperty("Email").GetValue(model).ToString() && x.Id != model.GetType().GetProperty("Id").GetValue(model).ToString());
                    if (itemexists.Count() > 0) return OperationResponse.Error(Common.Util.UserEmailAlreadyExists);
                }
                else if (model.GetType().Name == "Role")
                {
                    var items = await _repository.GetAllAsync();
                    var itemexists = ((List<Role>)items).Where(x => x.Name == model.GetType().GetProperty("Name").GetValue(model).ToString() && x.Id != model.GetType().GetProperty("Id").GetValue(model).ToString());
                    if (itemexists.Count() > 0) return OperationResponse.Error(Common.Util.RoleNameAlreadyExists);
                }
                await _repository.UpdateAsync(id, model);
                return OperationResponse.Success(Common.Util.RecordUpdatedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<IOperationResponse> CreateBulkAsync(IEnumerable<T> models)
        {
            await _repository.InsertBulkAsync(models);
            return OperationResponse.Success(Common.Util.BuildRecordsInsertedSuccessfully);
        }
        public virtual async Task<IOperationResponse> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Count() > 0
                ? OperationResponse<IEnumerable<T>>.Success(items, items.Count())
                : OperationResponse.NotFound(Common.Util.NoRecordsFound);
        }
        public async Task<IOperationResponse> GetAllWithPaginationAsync(DynamicTable dynamicTable)
        {
            var (items, itemcount) = await _repository.GetAllWithPaginationAsync(dynamicTable);
            return items.Count() > 0
                ? OperationResponse<IEnumerable<T>>.Success(items, itemcount)
                : OperationResponse.NotFound(Common.Util.NoRecordsFound);
        }
        public virtual async Task<IOperationResponse> GetByIdAsync(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item != null
                ? OperationResponse<T>.Success(item)
                : OperationResponse.NotFound(Common.Util.NoRecordsFound);
        }
        public async Task<IOperationResponse> GetByFilterAsync(string column, string id)
        {
            var item = await _repository.GetByFilterAsync(column, id);
            return item != null
                ? OperationResponse<T>.Success(item.FirstOrDefault())
                : OperationResponse.NotFound(Common.Util.NoRecordsFound);
        }
        public virtual async Task<IOperationResponse> DeleteAsync(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                var canDeleteProperty = item.GetType().GetProperty("CanDelete");
                if (canDeleteProperty != null && !(bool)canDeleteProperty.GetValue(item))
                {
                    return OperationResponse.Error($"{item.GetType().Name} cannot be deleted");
                }
                await _repository.DeleteAsync(id);
                return OperationResponse.Success($"{item.GetType().Name} deleted");
            }
            else
            {
                return OperationResponse.NotFound(Common.Util.InvalidId);
            }
        }
    }
}