using AutoMapper;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.DataAccess;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class CheckSheetVersionService : BaseService<CheckSheetVersion>, ICheckSheetVersionService
    {
        private readonly ICheckSheetVersionRepository _repository;
        private readonly IRepository<CheckSheet> _checkSheetRepository;
        private readonly ICacheService _cache;
        private readonly IMapper _mapper;
        private readonly IOptions<AppSettings> _options;
        private readonly IConfigurationService _configurationService;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IMasterSettingsService _masterSettingsService;
        private readonly IRepository<CheckSheetEmail> _emailRepository;
        public CheckSheetVersionService(ICheckSheetVersionRepository repository, IMapper mapper, ICacheService cache, IOptions<AppSettings> options,
                IConfigurationService configurationService, ILogger<CheckSheetVersionService> logger, IUserService userService, 
                IRepository<CheckSheet> checkSheetRepository, IMasterSettingsService masterSettingsService, IRepository<CheckSheetEmail> emailRepository) : base(repository, mapper)
        {
            _repository = repository;
            _cache = cache;
            _mapper = mapper;
            _options = options;
            _configurationService = configurationService;
            _userService = userService;
            _checkSheetRepository = checkSheetRepository;
            _logger = logger;
            _masterSettingsService = masterSettingsService;
            _emailRepository = emailRepository;
        }
        public async Task<IOperationResponse> GetAllAsync(string globalSearch)
        {
            var checkSheetVersions = await _repository.GetAllAsync();

            var items = await ToDTO(checkSheetVersions);

            if (!string.IsNullOrEmpty(globalSearch))
            {
                var searchTerms = globalSearch.Split(',', StringSplitOptions.TrimEntries);
                items = items.Where(x =>
                           searchTerms.Any(term =>
                                   (!string.IsNullOrEmpty(x.Name) && x.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.ChangeDetails) && x.ChangeDetails.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Department) && x.Department.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Equipment) && x.Equipment.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.EquipmentCode) && x.EquipmentCode.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Revision) && x.Revision.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Line) && x.Line.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Station) && x.Station.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Location) && x.Location.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.SubLocation) && x.SubLocation.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.UniqueId) && x.UniqueId.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Status) && x.Status.Contains(term, StringComparison.OrdinalIgnoreCase))));
            }

            return items.Any()
                ? OperationResponse<IEnumerable<CheckSheetVersionDTO>>.Success(items.OrderByDescending(x => x.CreatedOn), items.Count())
                : OperationResponse.Error(Common.Util.NoRecordsFound);
        }
        public async Task<IOperationResponse> GetLatestVersionsAsync(string globalSearch)
        {
            var checkSheetVersions = await _repository.GetLatestVersionsAsync();

            var items = await ToDTO(checkSheetVersions);

            if (!string.IsNullOrEmpty(globalSearch))
            {
                var searchTerms = globalSearch.Split(',', StringSplitOptions.TrimEntries);
                items = items.Where(x =>
                           searchTerms.Any(term =>
                                   (!string.IsNullOrEmpty(x.Name) && x.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.ChangeDetails) && x.ChangeDetails.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Department) && x.Department.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Equipment) && x.Equipment.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.EquipmentCode) && x.EquipmentCode.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Revision) && x.Revision.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Line) && x.Line.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Station) && x.Station.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Location) && x.Location.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.SubLocation) && x.SubLocation.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.UniqueId) && x.UniqueId.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Status) && x.Status.Contains(term, StringComparison.OrdinalIgnoreCase))));
            }

            return items.Any()
                ? OperationResponse<IEnumerable<CheckSheetVersionDTO>>.Success(items.OrderByDescending(x => x.CreatedOn), items.Count())
                : OperationResponse.Error(Common.Util.NoRecordsFound);
        }
        private async Task<IEnumerable<CheckSheetVersionDTO>> ToDTO(IEnumerable<CheckSheetVersion> checkSheetVersionItems, bool includeCheckPoints = false)
        {
            var configurations = ((OperationResponse<IEnumerable<ConfigurationDTO>>)await _configurationService.GetAllAsync()).Payload;

            var CheckSheetVersionDTOs = _mapper.Map<List<CheckSheetVersionDTO>>(checkSheetVersionItems);
            foreach (var checkSheetVersionDTO in CheckSheetVersionDTOs)
            {
                if (string.IsNullOrEmpty(checkSheetVersionDTO.Name)) checkSheetVersionDTO.Name = "New";
                UpdateDTO(checkSheetVersionDTO, configurations);
            }

            return CheckSheetVersionDTOs;
        }
        public static void UpdateDTO(CheckSheetVersionDTO checkSheetVersionDTO, IEnumerable<ConfigurationDTO> configurations)
        {
            var departmentConfig = configurations.FirstOrDefault(c => c.Id == checkSheetVersionDTO.DepartmentId);
            if (departmentConfig != null)
            {
                checkSheetVersionDTO.Department = departmentConfig.Description;
            }
            var equipmentConfig = configurations.FirstOrDefault(c => c.Id == checkSheetVersionDTO.EquipmentId);
            if (equipmentConfig != null)
            {
                checkSheetVersionDTO.Equipment = equipmentConfig.Description;
                checkSheetVersionDTO.EquipmentCode = equipmentConfig.Code;
            }
            var lineConfig = configurations.FirstOrDefault(c => c.Id == checkSheetVersionDTO.LineId);
            if (lineConfig != null)
            {
                checkSheetVersionDTO.Line = lineConfig.Description;
            }
            var maintenaceClassConfig = configurations.FirstOrDefault(c => c.Id == checkSheetVersionDTO.MaintenaceClassId);
            if (maintenaceClassConfig != null)
            {
                checkSheetVersionDTO.MaintenaceClass = maintenaceClassConfig.Description;
            }
            var stationConfig = configurations.FirstOrDefault(c => c.Id == checkSheetVersionDTO.StationId);
            if (stationConfig != null)
            {
                checkSheetVersionDTO.Station = stationConfig.Description;
            }
            var locationConfig = configurations.FirstOrDefault(c => c.Id == checkSheetVersionDTO.LocationId);
            if (locationConfig != null)
            {
                checkSheetVersionDTO.Location = locationConfig.Description;
            }
            var subLocationConfig = configurations.FirstOrDefault(c => c.Id == checkSheetVersionDTO.SubLocationId);
            if (subLocationConfig != null)
            {
                checkSheetVersionDTO.SubLocation = subLocationConfig.Description;
            }
        }
        public async Task<IOperationResponse> GetCheckSheetByIdAsync(string checkSheetId, UserDTO user)
        {
            var checkSheetVersion = await _repository.GetByIdAsync(checkSheetId);

            if (checkSheetVersion == null) return OperationResponse.Error(Common.Util.CheckSheetNotFound);

            var configurations = ((OperationResponse<IEnumerable<ConfigurationDTO>>)await _configurationService.GetAllAsync()).Payload;

            var checkSheet = _mapper.Map<CheckSheetVersionDTO>(checkSheetVersion);

            UpdateDTO(checkSheet, configurations);

            var userActions = new ManageCheckSheetUserActionDTO();
            userActions.IsReadOnly = checkSheet.Status != "In Progress" && checkSheet.Status != "Rejected" ? true : user.Role == "SuperAdmin" ? false :
                            (checkSheet.WorkFlowStage == user.Role && checkSheet.WorkFlowUser == user.FullName) ? false : true;

            userActions.ShowAddCheckPoint = userActions.IsReadOnly ? false : (checkSheet.WorkFlowStage == user.Role && checkSheet.WorkFlowUser == user.FullName) || user.Role == "SuperAdmin" ? true : false;

            userActions.ShowApprove = checkSheet.Status == "Approved" ? false : (checkSheet.WorkFlowStage == "Approver" && user.Role == "SuperAdmin") ? true :
                            (checkSheet.WorkFlowStage == "Approver" && checkSheet.WorkFlowUser == user.FullName) ? true : false;
            userActions.ShowReview = checkSheet.Status == "Approved" ? false : (checkSheet.WorkFlowStage == "Reviewer" && user.Role == "SuperAdmin") ? true :
                            (checkSheet.WorkFlowStage == "Reviewer" && checkSheet.WorkFlowUser == user.FullName) ? true : false;

            userActions.ShowCreateNewVersion = (checkSheet.Status == "Approved" && (user.Role == "Creator" || user.Role == "SuperAdmin")) ? true : false;
            userActions.ShowReplicateCheckSheet = userActions.ShowCreateNewVersion;

            userActions.ShowReject = checkSheet.Status == "Approved" || checkSheet.Status == "Rejected" || checkSheet.Status == "In Progress" ? false : user.Role == "SuperAdmin" ? true :
                            (checkSheet.WorkFlowStage == "Approver" && checkSheet.WorkFlowUser == user.FullName) ? true :
                            (checkSheet.WorkFlowStage == "Reviewer" && checkSheet.WorkFlowUser == user.FullName) ? true : false;

            userActions.ShowSubmit = checkSheet.Status == "Approved" ? false : ((checkSheet.Status == "Rejected" || checkSheet.Status == "In Progress") && user.Role == "SuperAdmin") ? true :
                            (checkSheet.WorkFlowStage == "Creator" && checkSheet.WorkFlowUser == user.FullName) ? true : false;

            userActions.ShowDelete = userActions.IsReadOnly ? false : (checkSheet.WorkFlowStage == user.Role && checkSheet.WorkFlowUser == user.FullName) || user.Role == "SuperAdmin" ? true : false;

            checkSheet.UserActions = userActions;
            return checkSheet != null
                ? OperationResponse<CheckSheetVersionDTO>.Success(checkSheet, 1)
                : OperationResponse.Error(Common.Util.NoRecordsFound);
        }
        public async Task<IOperationResponse> CreateAsync(CheckSheetCreateDTO checkSheetCreateDTO, UserDTO user)
        {
            ValidationContext context = new ValidationContext(checkSheetCreateDTO, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(checkSheetCreateDTO, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                if (checkSheetCreateDTO.Reviewers == null || checkSheetCreateDTO.Reviewers.Count() == 0) return OperationResponse.Error(Common.Util.AtLeastOneReviewerShouldBeSelected);
                if (checkSheetCreateDTO.Approvers == null || checkSheetCreateDTO.Approvers.Count() == 0) return OperationResponse.Error(Common.Util.AtLeastOneApproverShouldBeSelected);
                var filters = new Dictionary<string, object>() { { "UniqueId", checkSheetCreateDTO.UniqueId.Trim() },
                                                                 { "StationId", checkSheetCreateDTO.StationId },
                                                                 { "LineId", checkSheetCreateDTO.LineId }};
                var isUniqueIdExists = (await _repository.GetByFilterAsync(filters)).FirstOrDefault();
                if (isUniqueIdExists != null) return OperationResponse.Error(Common.Util.ControlNoStationLineCombinationExists);

                var checkSheetVersion = new CheckSheetVersion();

                checkSheetVersion.UniqueId = checkSheetCreateDTO.UniqueId;
                checkSheetVersion.StationId = checkSheetCreateDTO.StationId;
                checkSheetVersion.LineId = checkSheetCreateDTO.LineId;
                checkSheetVersion.Reviewers = _mapper.Map<List<Reviewer>>(checkSheetCreateDTO.Reviewers);
                checkSheetVersion.Approvers = _mapper.Map<List<Approver>>(checkSheetCreateDTO.Approvers);

                int sqOrder = 1;
                foreach (var item in checkSheetVersion.Reviewers)
                {
                    item.SeqOrder = sqOrder;
                    sqOrder++;
                }
                sqOrder = 1;
                foreach (var item in checkSheetVersion.Approvers)
                {
                    item.SeqOrder = sqOrder;
                    sqOrder++;
                }

                checkSheetVersion.CreatedBy = user.FullName;
                checkSheetVersion.CreatedByEmail = user.Email;
                checkSheetVersion.CreatedOn = Common.Util.GetISTLocalDate();
                checkSheetVersion.IsActivated = false;
                checkSheetVersion.ActivateOn = Common.Util.GetISTLocalDate().AddDays(5).Date;
                checkSheetVersion.WorkFlowStage = "Creator";
                checkSheetVersion.WorkFlowUser = user.FullName;
                await _repository.CreateAsync(checkSheetVersion);

                return checkSheetVersion != null
                    ? OperationResponse<CheckSheetVersion>.Success(checkSheetVersion, Common.Util.CheckSheetCreatedSuccessfully)
                    : OperationResponse.Error(Common.Util.NoRecordsFound);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<IOperationResponse> CreateVersionAsync(CheckSheetCreateVersionDTO checkSheetCreateVersionDTO, UserDTO user)
        {
            ValidationContext context = new ValidationContext(checkSheetCreateVersionDTO, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(checkSheetCreateVersionDTO, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                if (checkSheetCreateVersionDTO.Reviewers == null || checkSheetCreateVersionDTO.Reviewers.Count() == 0) return OperationResponse.Error(Common.Util.AtLeastOneReviewerShouldBeSelected);
                if (checkSheetCreateVersionDTO.Approvers == null || checkSheetCreateVersionDTO.Approvers.Count() == 0) return OperationResponse.Error(Common.Util.AtLeastOneApproverShouldBeSelected);

                var item = await _repository.GetByIdAsync(checkSheetCreateVersionDTO.Id);
                if (item == null) return OperationResponse.Error($"checksheet with the id {checkSheetCreateVersionDTO.Id} doesnt exist");

                var filters = new Dictionary<string, object>() { { "UniqueId", item.UniqueId },
                                                                  { "StationId", item.StationId },
                                                                  { "LineId", item.LineId } };

                var exisintCheckSheets = (await _repository.GetByFilterAsync(filters));
                var version = exisintCheckSheets.Count() + 1;

                item.Id = ObjectId.GenerateNewId().ToString();
                item.Version = version;
                item.Revision = Common.Util.GetRevisionText(version - 1);
                item.IsApproved = false;
                item.IsReviewed = false;
                item.Status = "In Progress";
                item.Reviewers = _mapper.Map<List<Reviewer>>(checkSheetCreateVersionDTO.Reviewers);
                item.Approvers = _mapper.Map<List<Approver>>(checkSheetCreateVersionDTO.Approvers);
                int sqOrder = 1;
                foreach (var reviewer in item.Reviewers)
                {
                    reviewer.SeqOrder = sqOrder;
                    sqOrder++;
                }
                sqOrder = 1;
                foreach (var approver in item.Approvers)
                {
                    approver.SeqOrder = sqOrder;
                    sqOrder++;
                }
                item.ChangeDetails = "";
                item.CreatedBy = user.FullName;
                item.CreatedByEmail = user.Email;
                item.CreatedOn = Common.Util.GetISTLocalDate();
                item.IsActivated = false;
                item.ActivateOn = Common.Util.GetISTLocalDate().AddDays(5).Date;
                item.WorkFlowStage = "Creator";
                item.WorkFlowUser = user.FullName;

                await _repository.CreateAsync(item);

                return item != null
                    ? OperationResponse<CheckSheetVersion>.Success(item, Common.Util.NewRevisionCreatedSuccessfully)
                    : OperationResponse.Error(Common.Util.NoRecordsFound);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<IOperationResponse> UpdateWorkFlowAsync(CheckSheetCreateVersionDTO checkSheetCreateVersionDTO, UserDTO user)
        {
            ValidationContext context = new ValidationContext(checkSheetCreateVersionDTO, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(checkSheetCreateVersionDTO, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                if (checkSheetCreateVersionDTO.Reviewers == null || checkSheetCreateVersionDTO.Reviewers.Count() == 0) return OperationResponse.Error(Common.Util.AtLeastOneReviewerShouldBeSelected);
                if (checkSheetCreateVersionDTO.Approvers == null || checkSheetCreateVersionDTO.Approvers.Count() == 0) return OperationResponse.Error(Common.Util.AtLeastOneApproverShouldBeSelected);

                var item = await _repository.GetByIdAsync(checkSheetCreateVersionDTO.Id);
                if (item == null) return OperationResponse.Error($"checksheet with the id {checkSheetCreateVersionDTO.Id} doesnt exist");

                if (item.Status != "In Progress" && item.Status != "Rejected") return OperationResponse.Error(Common.Util.ReviewersCanBeChangedOnlyForInProgressAndRejectedCheckSheets);

                item.Reviewers = _mapper.Map<List<Reviewer>>(checkSheetCreateVersionDTO.Reviewers);
                item.Approvers = _mapper.Map<List<Approver>>(checkSheetCreateVersionDTO.Approvers);
                int sqOrder = 1;
                foreach (var reviewer in item.Reviewers)
                {
                    reviewer.SeqOrder = sqOrder;
                    sqOrder++;
                }
                sqOrder = 1;
                foreach (var approver in item.Approvers)
                {
                    approver.SeqOrder = sqOrder;
                    sqOrder++;
                }
                item.ModifiedBy = user.FullName;
                item.ModifiedOn = Common.Util.GetISTLocalDate();

                await _repository.UpdateAsync(item.Id, item);

                return item != null
                    ? OperationResponse<CheckSheetVersion>.Success(item, Common.Util.WorkflowUpdatedSuccessfully)
                    : OperationResponse.Error(Common.Util.NoRecordsFound);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<IOperationResponse> ReplicateAsync(CheckSheetReplicateDTO checkSheetReplicateDTO, UserDTO user)
        {
            ValidationContext context = new ValidationContext(checkSheetReplicateDTO, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(checkSheetReplicateDTO, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                if (checkSheetReplicateDTO.Reviewers == null || checkSheetReplicateDTO.Reviewers.Count() == 0) return OperationResponse.Error(Common.Util.AtLeastOneReviewerShouldBeSelected);
                if (checkSheetReplicateDTO.Approvers == null || checkSheetReplicateDTO.Approvers.Count() == 0) return OperationResponse.Error(Common.Util.AtLeastOneApproverShouldBeSelected);

                var filters = new Dictionary<string, object>() { { "UniqueId", checkSheetReplicateDTO.UniqueId.Trim() },
                                                                 { "StationId", checkSheetReplicateDTO.StationId },
                                                                 { "LineId", checkSheetReplicateDTO.LineId }};
                var isUniqueIdExists = (await _repository.GetByFilterAsync(filters)).FirstOrDefault();
                if (isUniqueIdExists != null) return OperationResponse.Error(Common.Util.ControlNoStationLineCombinationExists);

                var item = await _repository.GetByIdAsync(checkSheetReplicateDTO.Id);
                if (item == null) return OperationResponse.Error($"checksheet with the id {checkSheetReplicateDTO.Id} doesnt exist");

                item.Id = ObjectId.GenerateNewId().ToString();
                item.UniqueId = checkSheetReplicateDTO.UniqueId;
                item.LineId = checkSheetReplicateDTO.LineId;
                item.StationId = checkSheetReplicateDTO.StationId;
                item.IsApproved = false;
                item.IsReviewed = false;
                item.Status = "In Progress";
                item.Reviewers = _mapper.Map<List<Reviewer>>(checkSheetReplicateDTO.Reviewers);
                item.Approvers = _mapper.Map<List<Approver>>(checkSheetReplicateDTO.Approvers);
                int sqOrder = 1;
                foreach (var reviewer in item.Reviewers)
                {
                    reviewer.SeqOrder = sqOrder;
                    sqOrder++;
                }
                sqOrder = 1;
                foreach (var approver in item.Approvers)
                {
                    approver.SeqOrder = sqOrder;
                    sqOrder++;
                }
                item.ChangeDetails = "";
                item.Version = 1;
                item.Revision = "New";
                item.CreatedBy = user.FullName;
                item.CreatedByEmail = user.Email;
                item.CreatedOn = Common.Util.GetISTLocalDate();
                item.IsActivated = false;
                item.ActivateOn = Common.Util.GetISTLocalDate().AddDays(5).Date;
                item.WorkFlowStage = "Creator";
                item.WorkFlowUser = user.FullName;
                await _repository.CreateAsync(item);

                return item != null
                    ? OperationResponse<CheckSheetVersion>.Success(item, Common.Util.CheckSheetReplicatedSuccessfully)
                    : OperationResponse.Error(Common.Util.NoRecordsFound);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        private bool IsConfigIdExists(IEnumerable<ConfigurationDTO> items, string Name, string Value)
        {
            var isExists = items.Where(x => x.Name == Name).Any(x => x.Id == Value);
            return isExists;
        }
        public async Task<IOperationResponse> UpdateCheckSheetAsync(string checkSheetId, CheckSheetVersionUpdateDTO checkSheetVersionUpdate, UserDTO user)
        {
            ValidationContext context = new ValidationContext(checkSheetVersionUpdate, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(checkSheetVersionUpdate, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                var configurationItems = ((IOperationResponse<IEnumerable<ConfigurationDTO>>)await _configurationService.GetAllAsync()).Payload;
                var checkSheet = await _repository.GetByIdAsync(checkSheetId);
                if (!IsConfigIdExists(configurationItems, "Department", checkSheetVersionUpdate.DepartmentId)) return OperationResponse.Error(Common.Util.DepartmentIdNotFound);
                if (!IsConfigIdExists(configurationItems, "Equipment", checkSheetVersionUpdate.EquipmentId)) return OperationResponse.Error(Common.Util.EquipmentIdNotFound);
                if (!IsConfigIdExists(configurationItems, "Line", checkSheetVersionUpdate.LineId)) return OperationResponse.Error(Common.Util.LineIdNotFound);
                if (!IsConfigIdExists(configurationItems, "Zone", checkSheetVersionUpdate.MaintenaceClassId)) return OperationResponse.Error(Common.Util.MaintenanceClassIdNotFound);
                if (!IsConfigIdExists(configurationItems, "Station", checkSheetVersionUpdate.StationId)) return OperationResponse.Error(Common.Util.StationIdNotFound);
                if (!IsConfigIdExists(configurationItems, "Location", checkSheetVersionUpdate.LocationId)) return OperationResponse.Error(Common.Util.LocationIdNotFound);
                if (!IsConfigIdExists(configurationItems, "SubLocation", checkSheetVersionUpdate.SubLocationId)) return OperationResponse.Error(Common.Util.SubLocationIdNotFound);


                var filters = new Dictionary<string, object>() { { "UniqueId", checkSheetVersionUpdate.UniqueId },
                                                                 { "StationId", checkSheetVersionUpdate.StationId },
                                                                 { "LineId", checkSheetVersionUpdate.LineId },
                                                                 { "Version", checkSheetVersionUpdate.Version } };
                var filterList = await _repository.GetByFilterAsync(filters);
                if (filterList != null)
                {
                    var itemExists = filterList.Any(x => x.Id != checkSheetVersionUpdate.Id);
                    if (itemExists)
                    {
                        return OperationResponse.Error(Common.Util.ControlNoStationLineCombinationExists);
                    }
                }

                checkSheet.Name = checkSheetVersionUpdate.Name;
                checkSheet.ChangeDetails = checkSheetVersionUpdate.ChangeDetails;
                checkSheet.DepartmentId = checkSheetVersionUpdate.DepartmentId;
                checkSheet.EquipmentId = checkSheetVersionUpdate.EquipmentId;
                checkSheet.LineId = checkSheetVersionUpdate.LineId;
                checkSheet.MaintenaceClassId = checkSheetVersionUpdate.MaintenaceClassId;
                checkSheet.StationId = checkSheetVersionUpdate.StationId;
                checkSheet.LocationId = checkSheetVersionUpdate.LocationId;
                checkSheet.SubLocationId = checkSheetVersionUpdate.SubLocationId;
                checkSheet.ActivateOn = checkSheetVersionUpdate.ActivateOn.Date;
                checkSheet.ModifiedBy = user.FullName;
                checkSheet.ModifiedOn = Common.Util.GetISTLocalDate();
                await _repository.UpdateAsync(checkSheetId, checkSheet);

                return OperationResponse.Success(Common.Util.UpdatedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<IOperationResponse> CreateCheckPointAsync(string checkSheetId, CheckPointDTO checkPointDTO)
        {
            ValidationContext context = new ValidationContext(checkPointDTO, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(checkPointDTO, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                if (checkPointDTO.FrequencyType == "Weekly" && (checkPointDTO.WeekDays.Count == 0 || checkPointDTO.WeekDays[0] == 0)) return OperationResponse.Error(Common.Util.WeekdaysCannotBeEmptyForWeekly);
                if (checkPointDTO.FrequencyType == "Monthly" && (checkPointDTO.MonthDays.Count == 0 || checkPointDTO.MonthDays[0] == 0)) return OperationResponse.Error(Common.Util.MonthDaysCannotBeEmptyForMonthly);
                if (checkPointDTO.FrequencyType == "Yearly"
                        && ((checkPointDTO.YearlyMonthDays.Count == 0 || checkPointDTO.YearlyMonths.Count == 0)
                        || (checkPointDTO.YearlyMonthDays[0] == 0 || checkPointDTO.YearlyMonths[0] == 0))) return OperationResponse.Error(Common.Util.YearlyMonthsYearlyMonthDaysCannotBeEmptyForYearly);

                var checkSheet = await _repository.GetByIdAsync(checkSheetId);
                if (checkSheet == null) return OperationResponse.Error($"checksheet with the id {checkSheetId} doesnt exist");

                if (checkSheet.CheckPoints !=null && checkSheet.CheckPoints.Any(x => x.SeqOrder == checkPointDTO.SeqOrder)) return OperationResponse.Error(Common.Util.SrNoAlreadyExists);

                var checkPoint = _mapper.Map<CheckPoint>(checkPointDTO);
                checkPoint.FrequencyText = Utils.MappingProfiles.GetFrequencyText(checkPoint);

                checkPoint.Id = ObjectId.GenerateNewId().ToString();
                if (checkSheet.CheckPoints == null)
                {
                    checkSheet.CheckPoints = new List<CheckPoint> { checkPoint };
                }
                else
                {
                    if (checkSheet.CheckPoints.Any(x => x.Name == checkPointDTO.Name)) return OperationResponse.Error(Common.Util.CheckpointNameAlreadyExists);
                    checkSheet.CheckPoints.Add(checkPoint);
                }
                await _repository.UpdateAsync(checkSheetId, checkSheet);

                return OperationResponse<CheckPoint>.Success(checkPoint, Common.Util.CheckPointCreatedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<IOperationResponse> UpdateCheckPointAsync(string checkSheetId, CheckPointDTO checkPointDTO)
        {
            ValidationContext context = new ValidationContext(checkPointDTO, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(checkPointDTO, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                if (checkPointDTO.FrequencyType == "Weekly" && (checkPointDTO.WeekDays.Count == 0 || checkPointDTO.WeekDays[0] == 0)) return OperationResponse.Error(Common.Util.WeekdaysCannotBeEmptyForWeekly);
                if (checkPointDTO.FrequencyType == "Monthly" && (checkPointDTO.MonthDays.Count == 0 || checkPointDTO.MonthDays[0] == 0)) return OperationResponse.Error(Common.Util.MonthDaysCannotBeEmptyForMonthly);
                if (checkPointDTO.FrequencyType == "Yearly"
                        && ((checkPointDTO.YearlyMonthDays.Count == 0 || checkPointDTO.YearlyMonths.Count == 0)
                        || (checkPointDTO.YearlyMonthDays[0] == 0 || checkPointDTO.YearlyMonths[0] == 0))) return OperationResponse.Error(Common.Util.YearlyMonthsYearlyMonthDaysCannotBeEmptyForYearly);

                var checkSheet = await _repository.GetByIdAsync(checkSheetId);
                if (checkSheet == null) return OperationResponse.Error($"checksheet with the id {checkSheetId} doesnt exist");

                var checkPoint = checkSheet.CheckPoints.Where(x => x.Id == checkPointDTO.Id).FirstOrDefault();
                if (checkPoint == null) return OperationResponse.Error($"checkpoint with the id {checkPointDTO.Id} doesnt exist");

                if (checkSheet.CheckPoints != null && checkSheet.CheckPoints.Any(x => x.Name == checkPointDTO.Name && x.Id != checkPointDTO.Id)) return OperationResponse.Error(Common.Util.CheckpointNameAlreadyExists);

                if (checkSheet.CheckPoints != null && checkSheet.CheckPoints.Any(x => x.SeqOrder == checkPointDTO.SeqOrder && x.Id != checkPointDTO.Id)) return OperationResponse.Error(Common.Util.SrNoAlreadyExists);

                _mapper.Map(checkPointDTO, checkPoint);
                checkPoint.FrequencyText = Utils.MappingProfiles.GetFrequencyText(checkPoint);
                await _repository.UpdateAsync(checkSheetId, checkSheet);

                return OperationResponse.Success(Common.Util.UpdatedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<CheckPoint> UpdateCheckSheetImageAsync(string checkSheetId, string checkPointId, string uniqueFileName, string fileName)
        {
            var item = await _repository.GetByIdAsync(checkSheetId);

            var checkPoint = item.CheckPoints.Where(x => x.Id == checkPointId).FirstOrDefault();
            checkPoint.UniqueFileName = uniqueFileName;
            checkPoint.FileName = fileName;
            await _repository.UpdateAsync(checkSheetId, item);

            return checkPoint;
        }
        public async Task<IOperationResponse> GetReviewersAndApproversAsync()
        {
            var configurationItems = ((IOperationResponse<IEnumerable<ConfigurationDTO>>)await _configurationService.GetAllAsync()).Payload;
            var users = ((IOperationResponse<IEnumerable<UserDTO>>)await _userService.GetAllAsync()).Payload;

            var reviewers = (from user in users
                             join department in configurationItems on user.DepartmentId equals department.Id into departmentGroup
                             from department in departmentGroup.DefaultIfEmpty()
                             where user.Role == "Reviewer"
                             select new ReviewerDTO
                             {
                                 Department = department.Description,
                                 IsReviewed = false,
                                 ReviewerId = user.Id,
                                 ReviewerName = user.FullName,
                                 Email = user.Email,
                                 SeqOrder = 1 // Initialize SeqOrder to 1
                             })
                            .Select((reviewer, index) => new ReviewerDTO
                            {
                                Department = reviewer.Department,
                                IsReviewed = reviewer.IsReviewed,
                                ReviewerId = reviewer.ReviewerId,
                                Email = reviewer.Email,
                                ReviewerName = reviewer.ReviewerName,
                                SeqOrder = index + 1 // Set SeqOrder to the index plus 1
                            });

            var approvers = (from user in users
                             join department in configurationItems on user.DepartmentId equals department.Id into departmentGroup
                             from department in departmentGroup.DefaultIfEmpty()
                             where user.Role == "Approver"
                             select new ApproverDTO
                             {
                                 Department = department.Description,
                                 IsApproved = false,
                                 ApproverId = user.Id,
                                 Email = user.Email,
                                 ApproverName = user.FullName,
                                 SeqOrder = 1
                             })
                            .Select((approver, index) => new ApproverDTO
                            {
                                Department = approver.Department,
                                IsApproved = approver.IsApproved,
                                ApproverId = approver.ApproverId,
                                Email = approver.Email,
                                ApproverName = approver.ApproverName,
                                SeqOrder = index + 1 // Set SeqOrder to the index plus 1
                            });

            var reviewersAndApprovers = new ReviewersAndApproversDTO
            {
                Reviewers = reviewers,
                Approvers = approvers,
            };
            return OperationResponse<ReviewersAndApproversDTO>.Success(reviewersAndApprovers);
        }
        public async Task<IOperationResponse> ApproveCheckSheetAsync(string checkSheetId, UserDTO user)
        {
            if (string.IsNullOrWhiteSpace(checkSheetId))
            {
                return OperationResponse.Error(Common.Util.PleaseEnterValidCheckSheetId);
            }
            var checkSheet = await _repository.GetByIdAsync(checkSheetId);

            if (checkSheet == null)
            {
                return OperationResponse.Error($"Check sheet with id {checkSheetId} not found");
            }

            if (checkSheet.Status != "Reviewed" && checkSheet.Status != "Partially Approved")
            {
                return OperationResponse.Error(Common.Util.OnlyReviewedCheckSheetsCanBeApproved);
            }

            if (checkSheet.Status == "Approved")
            {
                return OperationResponse.Error(Common.Util.CheckSheetAlreadyApproved);
            }

            if (checkSheet.Reviewers.Count() != checkSheet.Reviewers.Count(x => x.IsReviewed))
            {
                return OperationResponse.Error(Common.Util.AllReviewersShouldReviewBeforeApproval);
            }

            var approver = checkSheet.Approvers.OrderBy(x => x.SeqOrder).Where(x => x.IsApproved == false).FirstOrDefault();
            if (approver == null)
            {
                return OperationResponse.Error(Common.Util.CheckSheetAlreadyApprovedByYou);
            }
            else if (approver.ApproverId != user.Id && user.Role != "SuperAdmin")
            {
                return OperationResponse.Error($"The user {approver.ApproverName} should complete the approvel");
            }

            if (approver.SeqOrder != 1)
            {
                var peerApprover = checkSheet.Reviewers.Where(x => x.SeqOrder == approver.SeqOrder - 1).FirstOrDefault();
                if (peerApprover == null) return OperationResponse.Error(Common.Util.PeerApproverNotFound);

                if (!peerApprover.IsReviewed) return OperationResponse.Error($"The checksheet should be approved by {peerApprover.ReviewerName}");
            }

            approver.IsApproved = true;
            approver.ApprovedOn = Common.Util.GetISTLocalDate();
            if (user.Role == "SuperAdmin")
            {
                approver.ApproverId = user.Id;
                approver.ApproverName = user.FullName;
                approver.Department = user.Department ?? "";
            }
            checkSheet.Status = checkSheet.Approvers.Count() == checkSheet.Approvers.Count(x => x.IsApproved) ? "Approved" : "Partially Approved";

            var approver2 = checkSheet.Approvers.Where(x => x.SeqOrder == approver.SeqOrder + 1).FirstOrDefault();

            var emailReviewersList = checkSheet.Reviewers
                .Where(x => x.IsReviewed && !string.IsNullOrEmpty(x.Email))
                .Select(x => x.Email)
                .Distinct();

            var emailApproversList = checkSheet.Approvers
                .Where(x => x.IsApproved && !string.IsNullOrEmpty(x.Email))
                .Select(x => x.Email)
                .Distinct();

            var combinedEmails = emailReviewersList.Union(emailApproversList).ToList();

            if (approver2 != null)
            {
                checkSheet.WorkFlowUser = approver2.ApproverName;
                checkSheet.WorkFlowStage = "Approver";
                await _repository.UpdateAsync(checkSheetId, checkSheet);
                if (!string.IsNullOrEmpty(approver2.Email))
                {
                    await SendEmail(checkSheet, approver2.Email, approver2.ApproverName, combinedEmails);
                }
                else
                {
                    _logger.LogInformation("There is no To Email List to send email");
                }
            }
            else
            {
                checkSheet.WorkFlowUser = string.Empty;
                checkSheet.WorkFlowStage = string.Empty;
                await _repository.UpdateAsync(checkSheetId, checkSheet);
                await SendEmail(checkSheet, checkSheet.CreatedByEmail, checkSheet.CreatedBy, combinedEmails);
            }

            return OperationResponse.Success(Common.Util.CheckSheetApprovedSuccessfully);
        }
        public async Task<IOperationResponse> ReviewCheckSheetAsync(string checkSheetId, UserDTO user)
        {
            if (user == null)
            {
                return OperationResponse.Error(Common.Util.InvalidUser);
            }

            if (string.IsNullOrWhiteSpace(checkSheetId))
            {
                return OperationResponse.Error(Common.Util.PleaseEnterValidCheckSheetId);
            }
            var checkSheet = await _repository.GetByIdAsync(checkSheetId);

            if (checkSheet == null)
            {
                return OperationResponse.Error($"Check sheet with id {checkSheetId} not found");
            }

            if (checkSheet.Status != "Submitted" && checkSheet.Status != "Partially Reviewed")
            {
                return OperationResponse.Error(Common.Util.OnlySubmittedOrPartiallyReviewedCheckSheetsCanBeReviewed);
            }

            if (checkSheet.Status == "Approved")
            {
                return OperationResponse.Error(Common.Util.ApprovedCheckSheetsCannotBeReviewed);
            }

            if (checkSheet.Status == "Reviewed")
            {
                return OperationResponse.Error(Common.Util.CheckSheetAlreadyReviewed);
            }

            var reviewer = checkSheet.Reviewers.OrderBy(x => x.SeqOrder).Where(x => x.IsReviewed == false).FirstOrDefault();
            if (reviewer == null)
            {
                return OperationResponse.Error(Common.Util.CheckSheetAlreadyReviewedByYou);
            }
            else if (reviewer.ReviewerId != user.Id && user.Role != "SuperAdmin")
            {
                return OperationResponse.Error($"The user {reviewer.ReviewerName} should complete the review");
            }

            if (reviewer.SeqOrder != 1)
            {
                var peerReviewer = checkSheet.Reviewers.Where(x => x.SeqOrder == reviewer.SeqOrder - 1).FirstOrDefault();
                if (peerReviewer == null) return OperationResponse.Error(Common.Util.PeerReviewerNotFound);

                if (!peerReviewer.IsReviewed) return OperationResponse.Error($"The checksheet should be reviewed by {peerReviewer.ReviewerName}");
            }
            reviewer.IsReviewed = true;
            reviewer.ReviewedOn = Common.Util.GetISTLocalDate();
            checkSheet.Status = checkSheet.Reviewers.Count() == checkSheet.Reviewers.Count(x => x.IsReviewed) ? "Reviewed" : "Partially Reviewed";
            if (user.Role == "SuperAdmin")
            {
                reviewer.ReviewerId = user.Id;
                reviewer.ReviewerName = user.FullName;
                reviewer.Department = user.Department ?? "";
            }
            var reviewer2 = checkSheet.Reviewers.Where(x => x.SeqOrder == reviewer.SeqOrder + 1).FirstOrDefault();

            var emailReviewersList = checkSheet.Reviewers
                .Where(x => x.IsReviewed && !string.IsNullOrEmpty(x.Email))
                .Select(x => x.Email)
                .Distinct();

            var emailApproversList = checkSheet.Approvers
                .Where(x => x.IsApproved && !string.IsNullOrEmpty(x.Email))
                .Select(x => x.Email)
                .Distinct();

            var combinedEmails = emailReviewersList.Union(emailApproversList).ToList();

            if (reviewer2 == null)
            {
                var approver = checkSheet.Approvers.Where(x => x.SeqOrder == 1).FirstOrDefault();
                checkSheet.WorkFlowUser = approver.ApproverName;
                checkSheet.WorkFlowStage = "Approver";
                await _repository.UpdateAsync(checkSheetId, checkSheet);

                if (!string.IsNullOrEmpty(approver.Email))
                {
                    await SendEmail(checkSheet, approver.Email, approver.ApproverName, combinedEmails);
                }
                else
                {
                    _logger.LogInformation("There is no To Email List to send email");
                }
            }
            else
            {
                checkSheet.WorkFlowUser = reviewer2.ReviewerName;
                checkSheet.WorkFlowStage = "Reviewer";
                await _repository.UpdateAsync(checkSheetId, checkSheet);

                if (!string.IsNullOrEmpty(reviewer2.Email))
                {
                    await SendEmail(checkSheet, reviewer2.Email, reviewer2.ReviewerName, combinedEmails);
                }
                else
                {
                    _logger.LogInformation("There is no To Email List to send email");
                }
            }

            return OperationResponse.Success(Common.Util.CheckSheetReviewedSuccessfully);
        }
        public async Task<IOperationResponse> RejectCheckSheetAsync(string checkSheetId, string reason, UserDTO user)
        {
            if (string.IsNullOrWhiteSpace(checkSheetId))
            {
                return OperationResponse.Error(Common.Util.PleaseEnterValidCheckSheetId);
            }
            if (string.IsNullOrEmpty(reason))
            {
                return OperationResponse.Error(Common.Util.PleaseEnterRejectionComments);
            }
            var checkSheet = await _repository.GetByIdAsync(checkSheetId);

            if (checkSheet == null)
            {
                return OperationResponse.Error($"Check sheet with id {checkSheetId} not found");
            }

            checkSheet.Status = "Rejected";
            checkSheet.RejectedBy = user.FullName;
            checkSheet.RejectedOn = Common.Util.GetISTLocalDate();
            checkSheet.IsRejected = true;
            checkSheet.RejectionComments = reason;
            checkSheet.WorkFlowUser = checkSheet.CreatedBy;
            checkSheet.WorkFlowStage = "Creator";
            var emailReviewersList = checkSheet.Reviewers
                .Where(x => x.IsReviewed && !string.IsNullOrEmpty(x.Email))
                .Select(x => x.Email)
                .Distinct();

            var emailApproversList = checkSheet.Approvers
                .Where(x => x.IsApproved && !string.IsNullOrEmpty(x.Email))
                .Select(x => x.Email)
                .Distinct();

            var combinedEmails = emailReviewersList.Union(emailApproversList).ToList();

            await _repository.UpdateAsync(checkSheetId, checkSheet);

            if (!string.IsNullOrEmpty(checkSheet.CreatedByEmail))
            {
                await SendEmail(checkSheet, checkSheet.CreatedByEmail, checkSheet.CreatedBy, combinedEmails);
            }
            else
            {
                _logger.LogInformation("There is no To Email List to send email");
            }

            return OperationResponse.Success(Common.Util.CheckSheetRejectedSuccessfully);
        }
        public async Task<IOperationResponse> SubmitCheckSheetAsync(string checkSheetId, string changeDetails, UserDTO user)
        {
            if (string.IsNullOrWhiteSpace(checkSheetId))
            {
                return OperationResponse.Error(Common.Util.PleaseEnterValidCheckSheetId);
            }
            if (string.IsNullOrWhiteSpace(changeDetails))
            {
                return OperationResponse.Error(Common.Util.PleaseEnterChangeDetails);
            }
            var checkSheet = await _repository.GetByIdAsync(checkSheetId);

            if (checkSheet == null)
            {
                return OperationResponse.Error($"Check sheet with id {checkSheetId} not found");
            }

            if (checkSheet.Status == "Submitted")
            {
                return OperationResponse.Error(Common.Util.CheckSheetAlreadySubmitted);
            }

            if (checkSheet.DepartmentId == null) return OperationResponse.Error(Common.Util.PleaseAddDepartmentId);
            if (checkSheet.CheckPoints == null || checkSheet.CheckPoints.Count() == 0) return OperationResponse.Error(Common.Util.PleaseAddAtLeastOneCheckpoint);

            checkSheet.Status = "Submitted";
            checkSheet.ChangeDetails = changeDetails;
            checkSheet.RejectedBy = string.Empty;
            checkSheet.RejectedOn = DateTime.MinValue;
            checkSheet.RejectionComments = string.Empty;
            var reviewer = checkSheet.Reviewers.Where(x => x.SeqOrder == 1).FirstOrDefault();
            checkSheet.WorkFlowUser = reviewer.ReviewerName;

            checkSheet.WorkFlowStage = "Reviewer";
            foreach (var reviewer1 in checkSheet.Reviewers)
            {
                reviewer1.ReviewedOn = DateTime.MinValue;
                reviewer1.IsReviewed = false;
            }
            foreach (var approver in checkSheet.Approvers)
            {
                approver.ApprovedOn = DateTime.MinValue;
                approver.IsApproved = false;
            }
            await _repository.UpdateAsync(checkSheetId, checkSheet);

            if (!string.IsNullOrEmpty(reviewer.Email))
            {
                await SendEmail(checkSheet, reviewer.Email, reviewer.ReviewerName, new List<string>());
            }
            else
            {
                _logger.LogInformation("There is no To Email List to send email");
            }

            return OperationResponse.Success(Common.Util.CheckSheetSubmittedSuccessfully);
        }
        public async Task<IOperationResponse> MoveApprovedCheckSheets(string api_key)
        {
            if (api_key != _options.Value.ApiKey)
            {
                return OperationResponse.Error(Common.Util.ApiKeyIncorrect);
            }

            var filters = new Dictionary<string, object>() { { "IsActivated", false }, { "Status", "Approved" } };
            var items = await _repository.GetByFilterAsync(filters);
            var approvedCheckSheets = items.Where(x => x.ActivateOn <= DateTime.UtcNow.Date + TimeSpan.Zero);
            if (approvedCheckSheets.Count() == 0) return OperationResponse.Success(Common.Util.NoApprovedCheckSheets);

            foreach (var checkSheet in approvedCheckSheets.OrderBy(x => x.CreatedOn))
            {
                var filters2 = new Dictionary<string, object>() { { "UniqueId", checkSheet.UniqueId },
                                                                 { "StationId", checkSheet.StationId },
                                                                 { "LineId", checkSheet.LineId } };

                var exisintCheckSheet = (await _checkSheetRepository.GetByFilterAsync(filters2)).FirstOrDefault();
                if (exisintCheckSheet == null)
                {
                    await _checkSheetRepository.CreateAsync(_mapper.Map<CheckSheet>(checkSheet));
                }
                else
                {
                    await _checkSheetRepository.DeleteAsync(exisintCheckSheet.Id);
                    await _checkSheetRepository.CreateAsync(_mapper.Map<CheckSheet>(checkSheet));
                }
                checkSheet.IsActivated = true;
                await _repository.UpdateAsync(checkSheet.Id, checkSheet);
            }

            return OperationResponse.Success($"{approvedCheckSheets.Count()} approved Check Sheets moved Successfully");
        }
        private async Task SendEmail(CheckSheetVersion checkSheetVersion, string emailToList, string name, List<string> emailCC)
        {
            var configurations = ((OperationResponse<IEnumerable<ConfigurationDTO>>)await _configurationService.GetAllAsync()).Payload;
            var users = ((OperationResponse<IEnumerable<UserDTO>>)await _userService.GetAllAsync()).Payload;
            var superadmin_emailList = users.Where(x => x.Role == "SuperAdmin" && !string.IsNullOrWhiteSpace(x.Email)).Select(x => x.Email).Distinct();
            var emailCCList = emailCC != null ? superadmin_emailList.Union(emailCC) : superadmin_emailList;
            var checkSheet = _mapper.Map<CheckSheetVersionDTO>(checkSheetVersion);
            UpdateDTO(checkSheet, configurations);

            var subject = $"[{_options.Value.Env}] Digital Check Sheet - {checkSheet.Name} - {((checkSheet.Status == "Submitted" || checkSheet.Status == "Partially Reviewed") ? "Submitted for Review" : (checkSheet.Status == "Reviewed" || checkSheet.Status == "Submitted for Partially Approved") ? "Submitted for Approval" : checkSheet.Status)}";
            var htmlTable = new StringBuilder();

            htmlTable.Append("<table border='1'>");
            htmlTable.Append("<tr style='background-color: #ccc; padding: 10px; border: 1px solid #ddd;'><th>Name</th><th>Value</th></tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Control No</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.UniqueId + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Check Sheet Name</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.Name + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Department</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.Department + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Line</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.Line + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Equipment</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.Equipment + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Equipment Number</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.EquipmentCode + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Maintenace Class</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.MaintenaceClass + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Station</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.Station + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Created By</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.CreatedBy + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Revision</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.Revision + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Created On</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + Common.Util.GetDateLongText(checkSheet.CreatedOn) + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Change Details</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.ChangeDetails ?? "" + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("<tr>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Activate On</td>");
            htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + Common.Util.GetDateLongText(checkSheet.ActivateOn) + "</td>");
            htmlTable.Append("</tr>");

            htmlTable.Append("</table>");

            var reviewerTable = new StringBuilder();
            if (checkSheet.Reviewers.Any())
            {
                reviewerTable.Append("<table border='1'>");
                reviewerTable.Append("<tr style='background-color: #ccc; padding: 10px; border: 1px solid #ddd;'><th>Sl.No.</th><th>Reviewer</th><th>Department</th><th>Is Reviewed?</th><th>Reviewed On</th></tr>");
                foreach (var reviewer in checkSheet.Reviewers)
                {
                    reviewerTable.Append("<tr>");
                    reviewerTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + reviewer.SeqOrder + "</td>");
                    reviewerTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + reviewer.ReviewerName + "</td>");
                    reviewerTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + reviewer.Department + "</td>");
                    reviewerTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + (reviewer.IsReviewed ? "Yes" : "No") + "</td>");
                    reviewerTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + (reviewer.ReviewedOn == DateTime.MinValue ? "" : Common.Util.GetDateLongText(reviewer.ReviewedOn)) + "</td>");
                    reviewerTable.Append("</tr>");
                }
                reviewerTable.Append("</table>");
            }
            else
            {
                reviewerTable.Append("<b>Note:</b> No reviewers found.");
            }

            var approverTable = new StringBuilder();
            if (checkSheet.Approvers.Any())
            {
                approverTable.Append("<table border='1'>");
                approverTable.Append("<tr style='background-color: #ccc; padding: 10px; border: 1px solid #ddd;'><th>Sl.No.</th><th>Approver</th><th>Department</th><th>Is Approved?</th><th>Approved On</th></tr>");
                foreach (var approver in checkSheet.Approvers)
                {
                    approverTable.Append("<tr>");
                    approverTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + approver.SeqOrder + "</td>");
                    approverTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + approver.ApproverName + "</td>");
                    approverTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + approver.Department + "</td>");
                    approverTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + (approver.IsApproved ? "Yes" : "No") + "</td>");
                    approverTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + (approver.ApprovedOn == DateTime.MinValue ? "" : Common.Util.GetDateLongText(approver.ApprovedOn)) + "</td>");
                    approverTable.Append("</tr>");
                }
                approverTable.Append("</table>");
            }
            else
            {
                approverTable.Append("<b>Note:</b> No reviewers found.");
            }

            var rejectionTable = new StringBuilder();
            if (!string.IsNullOrEmpty(checkSheet.RejectedBy))
            {
                rejectionTable.Append("<table border='1'>");
                rejectionTable.Append("<tr style='background-color: #ccc; padding: 10px; border: 1px solid #ddd;'><th>Rejected By</th><th>Rejected On</th><th>Rejection Comments</th></tr>");
                rejectionTable.Append("<tr>");
                rejectionTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.RejectedBy + "</td>");
                rejectionTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + Common.Util.GetDateLongText(checkSheet.RejectedOn) + "</td>");
                rejectionTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.RejectionComments + "</td>");
                rejectionTable.Append("</tr>");
                rejectionTable.Append("</table>");
            }
            else
            {
                rejectionTable.Append("");
            }

            var body = File.ReadAllText("ManageCheckSheet_WorkFlowTemplate.txt")
                            .Replace("@CheckSheet@", htmlTable.ToString())
                            .Replace("@Status@", checkSheet.Status)
                            .Replace("@Name@", name)
                            .Replace("@Reviewers@", reviewerTable.ToString())
                            .Replace("@Approvers@", approverTable.ToString())
                            .Replace("@Rejection@", (rejectionTable.Length == 0 ? "" : rejectionTable.ToString()))
                            .Replace("@URL@", _options.Value.URL);

            var checkSheetEmail = new CheckSheetEmail { Subject = subject, Body = body, Env = _options.Value.Env, ToList = string.Join(",", emailToList), CcList = string.Join(",", emailCCList) };
            await _emailRepository.CreateAsync(checkSheetEmail);
        }
        public async Task<IOperationResponse> DeleteCheckPointAsync(string checkSheetId, string id)
        {
            var item = await _repository.GetByIdAsync(checkSheetId);
            if (item != null)
            {
                var checkPoint = item.CheckPoints.Where(x => x.Id == id).FirstOrDefault();
                if (checkPoint == null) return OperationResponse.NotFound(Common.Util.InvalidCheckSheetId);
                item.CheckPoints.Remove(checkPoint);
                await _repository.UpdateAsync(checkSheetId, item);
                return OperationResponse.Success($"{checkPoint.Name} deleted");
            }
            else
            {
                return OperationResponse.NotFound(Common.Util.InvalidCheckSheetId);
            }
        }
        public override async Task<IOperationResponse> DeleteAsync(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                if (item.Status == "In Progress")
                {
                    await _repository.DeleteAsync(id);
                    return OperationResponse.Success($"{item.Name} deleted");
                }
                else
                {
                    return OperationResponse.Error(Common.Util.OnlyInProgressCheckSheetsCanBeDeleted);
                }
            }
            else
            {
                return OperationResponse.NotFound(Common.Util.InvalidCheckSheetId);
            }
        }
    }
}
