using AutoMapper;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.DataAccess;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class CheckSheetService : BaseService<CheckSheet>, ICheckSheetService
    {
        private readonly IRepository<CheckSheet> _repository;
        private readonly ICheckSheetTransactionRepository _checkSheetTransactionRepository;
        private readonly IRepository<Configuration> _configurationRepository;
        private readonly IRepository<MasterSettings> _settingsRepository;
        private readonly IMapper _mapper;
        private readonly IOptions<AppSettings> _options;
        public CheckSheetService(IRepository<CheckSheet> repository, IMapper mapper, ICheckSheetTransactionRepository checkSheetTransactionRepository,
            IRepository<Configuration> configurationRepository, IRepository<MasterSettings> settingsRepository, IOptions<AppSettings> options) : base(repository, mapper)
        {
            _repository = repository;
            _checkSheetTransactionRepository = checkSheetTransactionRepository;
            _configurationRepository = configurationRepository;
            _mapper = mapper;
            _settingsRepository = settingsRepository;
            _options = options;
        }
        public async Task<IOperationResponse> GenerateDailyCheckSheetsAsync(string api_key)
        {
            if (api_key != _options.Value.ApiKey)
            {
                return OperationResponse.Error(Common.Util.ApiKeyIncorrect);
            }
            var checkSheetsExits = await _checkSheetTransactionRepository.GetByFilterAnyAsync("CheckSheetDay", DateTime.UtcNow.Date + TimeSpan.Zero);
            if (checkSheetsExits)
            {
                return OperationResponse.Error(Common.Util.CheckSheetsAlreadyGeneratedForToday);
            }

            var configurationItems = await _configurationRepository.GetAllAsync();
            var checkSheets = await _repository.GetAllAsync();
            if (checkSheets.Count() == 0)
            {
                return OperationResponse.Error(Common.Util.NoCheckSheetsFound);
            }

            var iCount = 0;
            var settings = await _settingsRepository.GetByIdAsync(Common.Util.SettngsId);
            if (settings != null)
            {
                foreach(var shift in settings.Shifts)
                {
                    var checkSheetTransactions = from checkSheet in checkSheets
                                                 join department in configurationItems on checkSheet.DepartmentId equals department.Id into departmentGroup
                                                 from department in departmentGroup.DefaultIfEmpty()
                                                 join line in configurationItems on checkSheet.LineId equals line.Id into lineGroup
                                                 from line in lineGroup.DefaultIfEmpty()
                                                 join station in configurationItems on checkSheet.StationId equals station.Id into stationGroup
                                                 from station in stationGroup.DefaultIfEmpty()
                                                 join equipment in configurationItems on checkSheet.EquipmentId equals equipment.Id into equipmentGroup
                                                 from equipment in equipmentGroup.DefaultIfEmpty()
                                                 join zone in configurationItems on checkSheet.MaintenaceClassId equals zone.Id into zoneGroup
                                                 from zone in zoneGroup.DefaultIfEmpty()
                                                 join location in configurationItems on checkSheet.LocationId equals location.Id into locationGroup
                                                 from location in locationGroup.DefaultIfEmpty()
                                                 join subLocation in configurationItems on checkSheet.SubLocationId equals subLocation.Id into subLocationGroup
                                                 from subLocation in subLocationGroup.DefaultIfEmpty()
                                                 select new CheckSheetTransaction
                                                 {
                                                     CheckPointTransactions = GetCheckPoints(checkSheet.CheckPoints),
                                                     CheckSheetDay = DateTime.UtcNow.Date + TimeSpan.Zero,
                                                     CheckSheetId = checkSheet.Id,
                                                     ColorCode = "black",
                                                     CreatedBy = "SYS",
                                                     CreatedOn = DateTime.UtcNow,
                                                     Department = department.Description,
                                                     DepartmentId = department.Id,
                                                     Equipment = equipment.Description,
                                                     EquipmentId = equipment.Id,
                                                     EquipmentCode = equipment.Code,
                                                     Location = location.Description,
                                                     SubLocation = subLocation.Description,
                                                     LocationId = location.Id,
                                                     SubLocationId = subLocation.Id,
                                                     IsActive = checkSheet.IsActive,
                                                     IsLocked = false,
                                                     Line = line.Description,
                                                     LineId = line.Id,
                                                     MaintenaceClass = zone.Description,
                                                     Name = checkSheet.Name,
                                                     Station = station.Description,
                                                     StationId = station.Id,
                                                     Status = "Not Started",
                                                     UniqueId = checkSheet.UniqueId,
                                                     Version = checkSheet.Version,
                                                     Revision = checkSheet.Revision,
                                                     ChangeDetails = checkSheet.ChangeDetails,
                                                     MaintenaceClassId = zone.Id,
                                                     Shift = shift.Name,
                                                     ShiftStartTime = shift.StartTime,
                                                     ShiftEndTime = shift.EndTime
                                                 };
                    var items = await _checkSheetTransactionRepository.CreateManyAsync(checkSheetTransactions);
                    iCount += items.Count();
                }                
            }
            
            return iCount > 0
                ? OperationResponse.Success($"{iCount} check sheets generated")
                : OperationResponse.Error(Common.Util.NoRecordsFound);
        }
        private List<CheckPointTransaction> GetCheckPoints(List<CheckPoint> checkPoints)
        {
            var checkPointTransactions = _mapper.Map<List<CheckPointTransaction>>(checkPoints);
            int todayDayOfWeek = (int)DateTime.Today.DayOfWeek;
            int todayDayOfMonth = DateTime.Today.Day;
            int todayMonth = DateTime.Today.Month;

            var filteredSheets = checkPointTransactions.Where(sheet =>
                (sheet.FrequencyType.Equals("Daily", StringComparison.OrdinalIgnoreCase)) ||
                (sheet.FrequencyType.Equals("Weekly", StringComparison.OrdinalIgnoreCase) && sheet.WeekDays.Contains(todayDayOfWeek)) ||
                (sheet.FrequencyType.Equals("Monthly", StringComparison.OrdinalIgnoreCase) && sheet.MonthDays.Contains(todayDayOfMonth)) ||
                (sheet.FrequencyType.Equals("Yearly", StringComparison.OrdinalIgnoreCase) && sheet.YearlyMonths.Contains(todayMonth) &&
                 sheet.YearlyMonthDays.Contains(todayDayOfMonth)));

            filteredSheets.ToList().ForEach(sheet => { sheet.IsForToday = true; });

            checkPointTransactions.Where(x => !x.IsForToday).ToList().ForEach(sheet => { sheet.CheckRecord = "NA"; });
            return checkPointTransactions;
        }
    }
}
