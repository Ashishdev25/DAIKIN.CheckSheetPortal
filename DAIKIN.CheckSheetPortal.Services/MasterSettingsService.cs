using AutoMapper;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using System.ComponentModel.DataAnnotations;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class MasterSettingsService : BaseService<MasterSettings>, IMasterSettingsService
    {
        private readonly IRepository<MasterSettings> _repository;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly string EntityName = "MasterSettings";
        public MasterSettingsService(IRepository<MasterSettings> repository, IMapper mapper, ICacheService cache) : base(repository, mapper)
        {
            _repository = repository;
            _cache = cache;
            _mapper = mapper;
        }
        public async Task<IOperationResponse> GetSettingsAsync()
        {
            var item = _cache.Get<MasterSettings>(EntityName);

            if (item == null)
            {
                item = await _repository.GetByIdAsync(Common.Util.SettngsId);
                _cache.Set(EntityName, item, TimeSpan.FromHours(12));
            }

            return item != null
                ? OperationResponse<MasterSettingsDTO>.Success(_mapper.Map<MasterSettingsDTO>(item))
                : OperationResponse.Error(Common.Util.SettingsNotFound);
        }
        public async Task<IOperationResponse> UpdateAsync(string id, MasterSettingsDTO model, UserDTO user)
        {
            var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            if (isValid)
            {
                var settings = _mapper.Map<MasterSettings>(model);
                settings.ModifiedBy = user.FullName;
                settings.ModifiedOn = Common.Util.GetISTLocalDate();
                await _repository.UpdateAsync(id, settings);
                _cache.Remove(EntityName);
                return OperationResponse<MasterSettings>.Success(settings, "MasterSettings updated successfully");
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
    }
}
