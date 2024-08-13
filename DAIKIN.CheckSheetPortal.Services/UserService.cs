using AutoMapper;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly ICacheService _cache;
        private readonly string EntityName = "User";
        private readonly IMapper _mapper;
        private readonly IConfigurationService _configurationService;
        public UserService(IRepository<User> repository, IMapper mapper, ICacheService cache, IConfigurationService configurationService) : base(repository, mapper)
        {
            _repository = repository;
            _cache = cache;
            _mapper = mapper;
            _configurationService = configurationService;
        }
        public override async Task<IOperationResponse> GetAllAsync()
        {
            var userDTOs = _cache.Get<IEnumerable<UserDTO>>(EntityName);

            if (userDTOs == null)
            {
                var users = await _repository.GetAllAsync();
                if (users.Count() > 0)
                {
                    var configurations_resp = await _configurationService.GetAllAsync();

                    if (!configurations_resp.IsSuccess) return OperationResponse.Error(Common.Util.NoConfigurationFound);

                    var configurations = ((OperationResponse<IEnumerable<ConfigurationDTO>>)configurations_resp).Payload;

                    userDTOs = _mapper.Map<List<UserDTO>>(users);
                    foreach (var userDTO in userDTOs)
                    {
                        var departmentConfig = configurations.FirstOrDefault(c => c.Id == userDTO.DepartmentId);
                        if (departmentConfig != null)
                        {
                            userDTO.Department = departmentConfig.Description;
                        }
                        var plantConfig = configurations.FirstOrDefault(c => c.Id == userDTO.PlantId);
                        if (plantConfig != null)
                        {
                            userDTO.Plant = plantConfig.Description;
                        }
                        userDTO.Lines = configurations.Where(c => userDTO.LineIds.Contains(c.Id)).Select(c => c.Description).ToList();
                    }

                    _cache.Set(EntityName, userDTOs, TimeSpan.FromHours(12));
                }
            }

            return userDTOs.Any()
                ? OperationResponse<IEnumerable<UserDTO>>.Success(userDTOs, userDTOs.Count())
                : OperationResponse.Error(Common.Util.NoRecordsFound);
        }
        public async Task<IOperationResponse> CreateAsync(UserDTO model, UserDTO loggedinuser)
        {
            var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            if (isValid)
            {
                var existingItems = await _repository.GetAllAsync();

                if (existingItems.Count() > 0 && existingItems.FirstOrDefault(x => x.LoginId == model.LoginId) != null)
                {
                    return OperationResponse.Error(Common.Util.LoginIdAlreadyExists);
                }
                model.Password = model.LoginId;
                model.CreatedBy = loggedinuser.FullName;
                model.CreatedOn = Common.Util.GetISTLocalDate();
                var user = _mapper.Map<User>(model);
                var item = await _repository.CreateAsync(user);
                _cache.Remove(EntityName);
                return OperationResponse<User>.Success(item, Common.Util.UserCreatedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<IOperationResponse> UpdateAsync(string id, UserDTO model, UserDTO loggedinuser)
        {
            var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

            if (isValid)
            {
                var existingItems = await _repository.GetAllAsync();

                if (existingItems.Count() > 0 && existingItems.FirstOrDefault(x => (x.LoginId == model.LoginId) && x.Id != id) != null)
                {
                    return OperationResponse.Error(Common.Util.LoginIdAlreadyExists);
                }
                var existingItem = existingItems.Where(X => X.Id == id).FirstOrDefault();
                model.Password = existingItem.Password;
                model.ModifiedBy = loggedinuser.FullName;
                model.ModifiedOn = Common.Util.GetISTLocalDate();

                var user = _mapper.Map<User>(model);
                await _repository.UpdateAsync(id, user);
                _cache.Remove(EntityName);
                return OperationResponse<User>.Success(user, Common.Util.UserUpdatedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public override async Task<IOperationResponse> DeleteAsync(string id)
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
        public async Task<UserDTO> GetByEmailAsync(string login)
        {
            var users = await _repository.GetByFilterAsync("LoginId", login);
            if (users.Count() == 0) return null;
            var user = users.FirstOrDefault();
            var configurations_resp = await _configurationService.GetAllAsync();

            var userDTO = _mapper.Map<UserDTO>(user);
            if (configurations_resp.IsSuccess)
            {
                var configurations = ((OperationResponse<IEnumerable<ConfigurationDTO>>)configurations_resp).Payload;
                userDTO.Plant = configurations.Where(x => x.Id == userDTO.PlantId).FirstOrDefault()?.Description ?? "";
                userDTO.Department = configurations.Where(x => x.Id == userDTO.DepartmentId).FirstOrDefault()?.Description ?? "";
            }
            return userDTO;
        }
        public async Task<IOperationResponse> UpdatePasswordAsync(string id, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword)) return OperationResponse.Error(Common.Util.NewPasswordAndConfirmPasswordRequired);

            if (newPassword != confirmPassword) return OperationResponse.Error(Common.Util.NewPasswordAndConfirmPasswordShouldBeTheSame);
            var user = await _repository.GetByIdAsync(id);

            if (user == null) return OperationResponse.Error(Common.Util.UserNotFound);

            user.Password = newPassword;
            await _repository.UpdateAsync(id, user);
            _cache.Remove(EntityName);
            return OperationResponse.Success(Common.Util.PasswordChangedSuccessfully);
        }
    }
}
