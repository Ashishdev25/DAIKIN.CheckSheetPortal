using AutoMapper;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using System.ComponentModel.DataAnnotations;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class ConfigurationService : BaseService<Configuration>, IConfigurationService
    {
        private readonly IRepository<Configuration> _repository;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly string EntityName = "Configuration";
        public ConfigurationService(IRepository<Configuration> repository, IMapper mapper, ICacheService cache) : base(repository, mapper)
        {
            _repository = repository;
            _cache = cache;
            _mapper = mapper;
        }
        public override async Task<IOperationResponse> GetAllAsync()
        {
            var items = _cache.Get<IEnumerable<Configuration>>(EntityName);

            if (items == null)
            {
                items = await _repository.GetAllAsync();
                _cache.Set(EntityName, items, TimeSpan.FromHours(12));
            }

            return items.Any()
                ? OperationResponse<IEnumerable<ConfigurationDTO>>.Success(_mapper.Map<IEnumerable<ConfigurationDTO>>(items), items.Count())
                : OperationResponse.Error(Common.Util.NoRecordsFound);
        }
        public async Task<IOperationResponse> CreateAsync(Configuration model, UserDTO user)
        {
            var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            if (isValid)
            {
                if (string.IsNullOrEmpty(model.Code) || string.IsNullOrEmpty(model.Description) || string.IsNullOrEmpty(model.Name)) return OperationResponse.Error(Common.Util.CodeAndDescriptionCannotBeEmpty);
                var existingItems = (await _repository.GetByFilterAsync("Name", model.Name)).ToList();

                if (existingItems.Count() > 0 && existingItems.FirstOrDefault(x => x.Code == model.Code && x.Description == model.Description) != null)
                {
                    return OperationResponse.Error(Common.Util.CodeAndDescriptionAlreadyExist);
                }
                model.CreatedBy = user.FullName;
                model.CreatedOn = Common.Util.GetISTLocalDate();
                var item = await _repository.CreateAsync(model);
                _cache.Remove(EntityName);
                return OperationResponse<Configuration>.Success(item, Common.Util.ConfigurationAddedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<IOperationResponse> UpdateAsync(string id, Configuration model, UserDTO user)
        {
            var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            if (isValid)
            {
                if (string.IsNullOrEmpty(model.Code) || string.IsNullOrEmpty(model.Description) || string.IsNullOrEmpty(model.Name)) return OperationResponse.Error(Common.Util.CodeAndDescriptionCannotBeEmpty);
                var existingItems = (await _repository.GetByFilterAsync("Name", model.Name)).ToList();

                if (existingItems.Count() > 0 && existingItems.FirstOrDefault(x => (x.Code == model.Code && x.Description == model.Description) && x.Id != id) != null)
                {
                    return OperationResponse.Error(Common.Util.CodeAndDescriptionAlreadyExist);
                }
                model.ModifiedBy = user.FullName;
                model.ModifiedOn = Common.Util.GetISTLocalDate();
                await _repository.UpdateAsync(id, model);
                _cache.Remove(EntityName);
                return OperationResponse<Configuration>.Success(model, Common.Util.ConfigurationUpdatedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public virtual async Task<IOperationResponse> DeleteAsync(string id, UserDTO user)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                if (!item.CanDelete) return OperationResponse.Error($"{EntityName} cannot be deleted");
                await _repository.DeleteAsync(id);
                _cache.Remove(EntityName);
                return OperationResponse.Success($"{EntityName} deleted");
            }
            else
            {
                return OperationResponse.NotFound(Common.Util.InvalidId);
            }
        }
    }
}
