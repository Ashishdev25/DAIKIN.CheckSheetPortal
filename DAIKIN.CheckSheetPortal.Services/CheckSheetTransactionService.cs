using AutoMapper;
using DAIKIN.CheckSheetPortal.Common;
using DAIKIN.CheckSheetPortal.DTO;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.DataAccess;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace DAIKIN.CheckSheetPortal.Services
{
    public class CheckSheetTransactionService : BaseService<CheckSheetTransaction>, ICheckSheetTransactionService
    {
        private readonly ICheckSheetTransactionRepository _repository;
        private readonly IRepository<CheckSheetTransactionArchive> _checkSheetArchiveRepository;
        private readonly IRepository<CheckSheetEmail> _emailRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly ICheckSheetImageService _imageService;
        private readonly IOptions<AppSettings> _options;
        private readonly IMasterSettingsService _masterSettingsService;
        public CheckSheetTransactionService(IUserService userService, ICheckSheetTransactionRepository repository, IRepository<User> userRepository, IRepository<CheckSheetEmail> emailRepository, IRepository<CheckSheetTransactionArchive> checkSheetArchiveRepository,
            IMapper mapper, ICheckSheetImageService imageService, ILogger<CheckSheetTransactionService> logger, IOptions<AppSettings> options, IMasterSettingsService masterSettingsService) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
            _imageService = imageService;
            _logger = logger;
            _userService = userService;
            _options = options;
            _masterSettingsService = masterSettingsService;
            _emailRepository = emailRepository;
            _checkSheetArchiveRepository = checkSheetArchiveRepository;
        }
        public async Task<IOperationResponse> GetTransactionsAsync(string globalSearch, bool searchByDate, DateTime fromDate, DateTime toDate)
        {
            if (!searchByDate) return OperationResponse.Error(Common.Util.PleaseSelectDateRange);

            if (fromDate == DateTime.MinValue || toDate == DateTime.MinValue) return OperationResponse.Error(Common.Util.FromDateAndToDateCannotBeEmpty);

            if ((toDate - fromDate).Duration().TotalDays > 10) return OperationResponse.Error(Common.Util.DateRangeWithin10Days);

            var items = await _repository.GetByBetweenDatesAsync("CheckSheetDay", fromDate.Date + TimeSpan.Zero, toDate.Date + TimeSpan.Zero);

            var archivedItems = await _checkSheetArchiveRepository.GetByBetweenDatesAsync("CheckSheetDay", fromDate.Date + TimeSpan.Zero, toDate.Date + TimeSpan.Zero);

            var combinedItems = items.Concat(archivedItems);

            if (!string.IsNullOrEmpty(globalSearch))
            {
                var searchTerms = globalSearch.Split(',', StringSplitOptions.TrimEntries);
                combinedItems = combinedItems.Where(x =>
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
                                   (!string.IsNullOrEmpty(x.Shift) && x.Shift.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.UniqueId) && x.UniqueId.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(x.Status) && x.Status.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                    x.CheckPointTransactions.Any(cp => cp.CheckRecord != null && cp.CheckRecord.Contains(term, StringComparison.OrdinalIgnoreCase))));
            }
            return combinedItems.Any()
                ? OperationResponse<IEnumerable<ViewCheckSheetTransactionDTO>>.Success(_mapper.Map<IEnumerable<ViewCheckSheetTransactionDTO>>(combinedItems), combinedItems.Count())
                : OperationResponse.Error(Common.Util.NoRecordsFound);
        }
        public async Task<IOperationResponse> GetCheckSheetsAsync(string globalSearch, UserDTO userDTO)
        {
            var items = await _repository.GetByFilterAsync("CheckSheetDay", DateTime.UtcNow.Date + TimeSpan.Zero);

            var currentTime = Util.GetISTLocalDate().ToString("HH:mm");
            items = items.Where(x =>
                (string.Compare(x.ShiftStartTime, x.ShiftEndTime) < 0 && string.Compare(x.ShiftStartTime, currentTime) < 0 && string.Compare(x.ShiftEndTime, currentTime) > 0) ||
                (string.Compare(x.ShiftStartTime, x.ShiftEndTime) > 0 && (string.Compare(x.ShiftStartTime, currentTime) < 0 || string.Compare(x.ShiftEndTime, currentTime) > 0))
            ).ToList();

            var users = ((IOperationResponse<IEnumerable<UserDTO>>)await _userService.GetAllAsync()).Payload;
            var user = users.Where(x => x.Id == userDTO.Id).FirstOrDefault();
            if (userDTO.Role != "SuperAdmin")
            {
                items = items.Where(x => x.DepartmentId == user.DepartmentId && user.LineIds.Contains(x.LineId));
            }

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
                                   (!string.IsNullOrEmpty(x.Status) && x.Status.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                    x.CheckPointTransactions.Any(cp => cp.CheckRecord != null && cp.CheckRecord.Contains(term, StringComparison.OrdinalIgnoreCase))));
            }

            if (!items.Any()) return OperationResponse.Error(Common.Util.NoRecordsFound);

            var dtos = _mapper.Map<IEnumerable<CheckSheetTransactionDTO>>(items);
            var badgecount = dtos.GroupBy(x => x.Status).ToDictionary(x => x.Key, y => y.Count());
            return OperationResponse<CheckSheetTransactionMaster>.Success(new CheckSheetTransactionMaster
            {
                CheckSheetTransaction = dtos,
                BadgeCount = badgecount
            }, items.Count());
        }
        public async Task<IOperationResponse> GetCheckSheetByIdAsync(string id, UserDTO user)
        {
            var checkSheet = await _repository.GetByIdAsync(id);
            var settings = ((OperationResponse<MasterSettingsDTO>)(await _masterSettingsService.GetSettingsAsync())).Payload;
            if (checkSheet == null) return OperationResponse.Error(Common.Util.NoRecordsFound);

            var badgeCount = checkSheet.CheckPointTransactions
                                                .Where(c => !string.IsNullOrEmpty(c.CheckRecord))
                                                .GroupBy(c => c.CheckRecord)
                                                .ToDictionary(group => group.Key, group => group.Count());


            if ((checkSheet.Status == "In Progress" || checkSheet.Status == "Rejected") && checkSheet.LockedBy != null && checkSheet.LockedBy != user.FullName && checkSheet.IsLocked
                && Util.GetISTLocalDate().Subtract(checkSheet.LockedOn ?? Util.GetISTLocalDate()).TotalMinutes <= settings.Locktime)
            {
                var userActions = new UserActionsDTO
                {
                    IsReadOnly = true,
                    ShowApproveButton = false,
                    ShowExportPrintVersion = true,
                    ShowSubmitButton = false,
                };
                return OperationResponse<CheckSheetTransactionFullMaster>.Success(new CheckSheetTransactionFullMaster
                {
                    CheckSheetTransaction = _mapper.Map<CheckSheetTransactionFullDTO>(checkSheet),
                    BadgeCount = badgeCount,
                    UserActions = userActions
                }, $"Check Sheet filling is in-process by {checkSheet.StartedBy}");
            }
            else
            {
                var userActions = new UserActionsDTO
                {
                    IsReadOnly = checkSheet.Status == "Approved" ? true : user.Role == "SuperAdmin" ? false : (user.Role == "Operator" && (checkSheet.Status == "In Progress" || checkSheet.Status == "Not Started")) ? false :
                                                                (user.Role == "Validator" && checkSheet.Status == "Submitted") ? false : true,
                    ShowApproveButton = checkSheet.Status == "Approved" ? false : (user.Role == "Validator" || user.Role == "SuperAdmin") && checkSheet.Status == "Submitted" ? true : false,
                    ShowExportPrintVersion = true,
                    ShowSubmitButton = checkSheet.Status == "Approved" ? false : ((user.Role == "Operator" || user.Role == "SuperAdmin") && (checkSheet.Status == "In Progress" || checkSheet.Status == "Not Started")) ? true : false,
                };

                return OperationResponse<CheckSheetTransactionFullMaster>.Success(new CheckSheetTransactionFullMaster
                {
                    CheckSheetTransaction = _mapper.Map<CheckSheetTransactionFullDTO>(checkSheet),
                    BadgeCount = badgeCount,
                    UserActions = userActions
                }, checkSheet.CheckPointTransactions.Count());
            }
        }
        private static async Task AddImageAsync(ExcelWorksheet worksheet, string imageName, byte[] imageInBytes, int row, int rowOffset, int column, int columnOffset)
        {
            using (var ms = new MemoryStream(imageInBytes))
            {
                var image = worksheet.Drawings.AddPicture(imageName, ms);
                image.SetPosition(row, rowOffset, column, columnOffset);
            }
        }
        public async Task<(MemoryStream, string)> ExportTransactionsAsync(bool exportWithCheckPoints, string globalSearch, bool searchByDate, DateTime fromDate, DateTime toDate)
        {
            if (!searchByDate) throw new Exception("Please select daterange");

            if (fromDate == DateTime.MinValue || toDate == DateTime.MinValue) throw new Exception("fromdate and todate cannot be empty");

            if ((toDate - fromDate).Duration().TotalDays > 10) throw new Exception("The date range should be within 10 days");

            var items = await _repository.GetByBetweenDatesAsync("CheckSheetDay", fromDate.Date + TimeSpan.Zero, toDate.Date + TimeSpan.Zero);

            var archivedItems = await _checkSheetArchiveRepository.GetByBetweenDatesAsync("CheckSheetDay", fromDate.Date + TimeSpan.Zero, toDate.Date + TimeSpan.Zero);

            var combinedItems = items.Concat(archivedItems);

            if (!string.IsNullOrEmpty(globalSearch))
            {
                var searchTerms = globalSearch.Split(',', StringSplitOptions.TrimEntries);
                combinedItems = combinedItems.Where(x =>
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
                                   (!string.IsNullOrEmpty(x.Status) && x.Status.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                                    x.CheckPointTransactions.Any(cp => cp.CheckRecord != null && cp.CheckRecord.Contains(term, StringComparison.OrdinalIgnoreCase))));
            }

            if (!combinedItems.Any())
            {
                throw new Exception("checksheets not found.");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var memoryStream = new MemoryStream();
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                var headerColumn = 1;
                worksheet.Cells[headerColumn, 1].Value = "Sr.No";
                worksheet.Cells[headerColumn, 2].Value = "Check Sheet Name";
                worksheet.Cells[headerColumn, 3].Value = "Control No";
                worksheet.Cells[headerColumn, 4].Value = "Department";
                worksheet.Cells[headerColumn, 5].Value = "Line";
                worksheet.Cells[headerColumn, 6].Value = "Maintenance Class";
                worksheet.Cells[headerColumn, 7].Value = "Station";
                worksheet.Cells[headerColumn, 8].Value = "Equipment";
                worksheet.Cells[headerColumn, 9].Value = "Equipment Code";
                worksheet.Cells[headerColumn, 10].Value = "Location";
                worksheet.Cells[headerColumn, 11].Value = "Sub Location";
                worksheet.Cells[headerColumn, 12].Value = "Shift";
                worksheet.Cells[headerColumn, 13].Value = "Revision";
                worksheet.Cells[headerColumn, 14].Value = "Check Sheet Day";
                worksheet.Cells[headerColumn, 15].Value = "Started By";
                worksheet.Cells[headerColumn, 16].Value = "Started At";
                worksheet.Cells[headerColumn, 17].Value = "Validated By";
                worksheet.Cells[headerColumn, 18].Value = "Validated On";
                worksheet.Cells[headerColumn, 19].Value = "Status";
                worksheet.Cells[headerColumn, 20].Value = "Sq Order";
                worksheet.Cells[headerColumn, 21].Value = "Check Point";
                worksheet.Cells[headerColumn, 22].Value = "Standard";
                worksheet.Cells[headerColumn, 23].Value = "Condition";
                worksheet.Cells[headerColumn, 24].Value = "Method";
                worksheet.Cells[headerColumn, 25].Value = "Complete In";
                worksheet.Cells[headerColumn, 26].Value = "Frequency";
                worksheet.Cells[headerColumn, 27].Value = "Check Record";
                worksheet.Cells[headerColumn, 28].Value = "Commments";

                using (var range = worksheet.Cells[$"A{headerColumn}:AA{headerColumn}"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }
                int rowCount = headerColumn + 1;
                foreach (var checkSheet in combinedItems)
                {
                    foreach (var checkPoint in checkSheet.CheckPointTransactions)
                    {
                        worksheet.Cells[rowCount, 1].Value = rowCount - headerColumn;
                        worksheet.Cells[rowCount, 2].Value = checkSheet.Name;
                        worksheet.Cells[rowCount, 3].Value = checkSheet.UniqueId;
                        worksheet.Cells[rowCount, 4].Value = checkSheet.Department;
                        worksheet.Cells[rowCount, 5].Value = checkSheet.Line;
                        worksheet.Cells[rowCount, 6].Value = checkSheet.MaintenaceClass;
                        worksheet.Cells[rowCount, 7].Value = checkSheet.Station;
                        worksheet.Cells[rowCount, 8].Value = checkSheet.Equipment;
                        worksheet.Cells[rowCount, 9].Value = checkSheet.EquipmentCode;
                        worksheet.Cells[rowCount, 10].Value = checkSheet.Location;
                        worksheet.Cells[rowCount, 11].Value = checkSheet.SubLocation;
                        worksheet.Cells[rowCount, 12].Value = checkSheet.Shift;
                        worksheet.Cells[rowCount, 13].Value = checkSheet.Revision;
                        worksheet.Cells[rowCount, 14].Value = checkSheet.CheckSheetDay;
                        worksheet.Cells[rowCount, 14].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                        worksheet.Cells[rowCount, 15].Value = checkSheet.StartedBy;
                        worksheet.Cells[rowCount, 16].Value = checkSheet.StartedOn;
                        worksheet.Cells[rowCount, 16].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                        worksheet.Cells[rowCount, 17].Value = checkSheet.ValidatedBy;
                        worksheet.Cells[rowCount, 18].Value = checkSheet.ValidatedOn;
                        worksheet.Cells[rowCount, 18].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                        worksheet.Cells[rowCount, 19].Value = checkSheet.Status;
                        worksheet.Cells[rowCount, 20].Value = checkPoint.SeqOrder;
                        worksheet.Cells[rowCount, 21].Value = checkPoint.Name;
                        worksheet.Cells[rowCount, 22].Value = checkPoint.Standard;
                        worksheet.Cells[rowCount, 23].Value = checkPoint.Condition;
                        worksheet.Cells[rowCount, 24].Value = checkPoint.Method;
                        worksheet.Cells[rowCount, 25].Value = checkPoint.CompleteInSeconds;
                        worksheet.Cells[rowCount, 26].Value = checkPoint.FrequencyType;
                        worksheet.Cells[rowCount, 27].Value = checkPoint.CheckRecord;
                        worksheet.Cells[rowCount, 28].Value = checkPoint.Comments;

                        var row = worksheet.Row(rowCount);
                        if (checkPoint.CheckRecord == "NG" || checkPoint.CheckRecord == "AbnormalCanUse")
                        {
                            row.Style.Font.Color.SetColor(Color.Red);
                        }
                        rowCount++;
                    }
                }
                worksheet.Cells[1, 1, rowCount - 1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                worksheet.Cells[1, 1, rowCount - 1, 28].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[1, 1, rowCount - 1, 28].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, rowCount - 1, 28].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, rowCount - 1, 28].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, rowCount - 1, 28].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = true;
                string headerRange = $"1:{headerColumn}";
                worksheet.PrinterSettings.RepeatRows = worksheet.Cells[headerRange];
                worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                worksheet.PrinterSettings.LeftMargin = 0.25M;
                worksheet.PrinterSettings.RightMargin = 0.25M;
                worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
                worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                worksheet.PrinterSettings.HorizontalCentered = true;
                worksheet.PrinterSettings.FitToPage = true;
                worksheet.PrinterSettings.FitToWidth = 1;
                worksheet.PrinterSettings.FitToHeight = 0;

                if (fromDate != DateTime.MinValue && !string.IsNullOrEmpty(globalSearch))
                {
                    worksheet.HeaderFooter.OddHeader.CenteredText = $"Daikin Check Sheet Transactions for the period from {fromDate.ToShortDateString()} to {toDate.ToShortDateString()} and with Global search: {globalSearch}";
                }
                else if (!string.IsNullOrEmpty(globalSearch))
                {
                    worksheet.HeaderFooter.OddHeader.CenteredText = $"Daikin Check Sheet Transactions with Global search: {globalSearch}";
                }
                else
                {
                    worksheet.HeaderFooter.OddHeader.CenteredText = $"Daikin Check Sheet Transactions for the period from {fromDate.ToShortDateString()} to {toDate.ToShortDateString()}";
                }
                worksheet.HeaderFooter.OddFooter.LeftAlignedText = "Date Generated: &D &T";
                worksheet.HeaderFooter.OddFooter.RightAlignedText = "Page: &P";
                worksheet.View.FreezePanes(headerColumn + 1, 1);
                excelPackage.Save();
            }
            var fileName = $"DAIKIN_DigitalCheckSheet_Transactions_{DateTime.UtcNow.AddMinutes(330).ToString("ddMMMyyyy")}.xlsx";
            memoryStream.Position = 0;
            return (memoryStream, fileName);
        }
        public async Task<(MemoryStream, string)> ExportCheckSheetByIdAsync(string id)
        {
            var memoryStream = new MemoryStream();
            var checkSheet = await _repository.GetByIdAsync(id);
            if (checkSheet == null) throw new Exception("checksheet not found.");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var excelPackage = new ExcelPackage(memoryStream))
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");

                worksheet.Cells["A1:I1"].Merge = true;
                worksheet.Cells["A1"].Value = $"DAIKIN - Digital Check Sheet - {checkSheet.Name}";

                worksheet.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                worksheet.Cells["A1"].Style.Font.Size = 20;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Row(1).Height = 30;

                worksheet.Cells["A2:B5"].Merge = true;
                var logoInBytes = await _imageService.DownloadAsync("common", "Logo.jpg", 120, 100);
                await AddImageAsync(worksheet, "logo", logoInBytes, 1, 30, 0, 20);

                worksheet.Cells[2, 3].Value = "Check Sheet Name";
                worksheet.Cells[2, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells["D2:E2"].Merge = true;
                worksheet.Cells[2, 4].Value = checkSheet.Name;
                worksheet.Cells[2, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 3].Value = "Control No";
                worksheet.Cells[3, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[3, 3].Style.Font.Bold = true;
                worksheet.Cells["D3:E3"].Merge = true;
                worksheet.Cells[3, 4].Value = checkSheet.UniqueId;
                worksheet.Cells[3, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[4, 3].Value = "Department";
                worksheet.Cells[4, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[4, 3].Style.Font.Bold = true;
                worksheet.Cells["D4:E4"].Merge = true;
                worksheet.Cells[4, 4].Value = checkSheet.Department;
                worksheet.Cells[4, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[5, 3].Value = "Equipment";
                worksheet.Cells[5, 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[5, 3].Style.Font.Bold = true;
                worksheet.Cells["D5:E5"].Merge = true;
                worksheet.Cells[5, 4].Value = checkSheet.Equipment;
                worksheet.Cells[5, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[2, 6].Value = "Maintenance Class";
                worksheet.Cells[2, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[2, 6].Style.Font.Bold = true;
                worksheet.Cells[2, 7].Value = checkSheet.MaintenaceClass;
                worksheet.Cells[2, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 6].Value = "Line";
                worksheet.Cells[3, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[3, 6].Style.Font.Bold = true;
                worksheet.Cells[3, 7].Value = checkSheet.Line;
                worksheet.Cells[3, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[4, 6].Value = "Equipment No";
                worksheet.Cells[4, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[4, 6].Style.Font.Bold = true;
                worksheet.Cells[4, 7].Value = checkSheet.EquipmentCode;
                worksheet.Cells[4, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[5, 6].Value = "Revision";
                worksheet.Cells[5, 6].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[5, 6].Style.Font.Bold = true;
                worksheet.Cells[5, 7].Value = checkSheet.Revision;
                worksheet.Cells[5, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[2, 8].Value = "Location";
                worksheet.Cells[2, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[2, 8].Style.Font.Bold = true;
                worksheet.Cells[2, 9].Value = checkSheet.Location;
                worksheet.Cells[2, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[3, 8].Value = "Sub Location";
                worksheet.Cells[3, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[3, 8].Style.Font.Bold = true;
                worksheet.Cells[3, 9].Value = checkSheet.SubLocation;
                worksheet.Cells[3, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[4, 8].Value = "Station";
                worksheet.Cells[4, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[4, 8].Style.Font.Bold = true;
                worksheet.Cells[4, 9].Value = checkSheet.Station;
                worksheet.Cells[4, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[5, 8].Value = "Date Created";
                worksheet.Cells[5, 8].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                worksheet.Cells[5, 8].Style.Font.Bold = true;
                worksheet.Cells[5, 9].Value = checkSheet.ModifiedOn?.AddMinutes(330).ToString("dd.MMM.yyyy HH:mm:ss");
                worksheet.Cells[5, 9].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells["A6:I6"].Merge = true;
                worksheet.Cells["A6"].Value = "Check Record Accepted Entries: OK - O, NG/Abnormal (can not use machine)- X, Abnormal (can use machine) - Δ, or Not Applicable - NA";
                worksheet.Cells["A6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                using (var range = worksheet.Cells["A6:I6"])
                {
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                }

                var headerColumn = 7;
                worksheet.Cells[headerColumn, 1].Value = "Sr.No";
                worksheet.Cells[headerColumn, 2].Value = "Name";
                worksheet.Cells[headerColumn, 3].Value = "Standard";
                worksheet.Cells[headerColumn, 4].Value = "Condition";
                worksheet.Cells[headerColumn, 5].Value = "Frequency";
                worksheet.Cells[headerColumn, 6].Value = "Method";
                worksheet.Cells[headerColumn, 7].Value = "Image";
                worksheet.Cells[headerColumn, 8].Value = "CheckRecord";
                worksheet.Cells[headerColumn, 9].Value = "Remarks - Only for NG or Abnormal (can use)";
                using (var range = worksheet.Cells[$"A{headerColumn}:I{headerColumn}"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }
                int rowCount = headerColumn + 1;
                foreach (var checkPoint in checkSheet.CheckPointTransactions)
                {
                    worksheet.Cells[rowCount, 1].Value = rowCount - headerColumn;
                    worksheet.Cells[rowCount, 2].Value = checkPoint.Name;
                    worksheet.Cells[rowCount, 3].Value = checkPoint.Standard;
                    worksheet.Cells[rowCount, 4].Value = checkPoint.Condition;
                    worksheet.Cells[rowCount, 5].Value = checkPoint.FrequencyText;
                    worksheet.Cells[rowCount, 6].Value = checkPoint.Method;

                    var imageInBytes = await _imageService.DownloadAsync("images", checkPoint.UniqueFileName, 100, 80);
                    await AddImageAsync(worksheet, Guid.NewGuid().ToString(), imageInBytes, rowCount - 1, 10, 6, 10);

                    worksheet.Row(rowCount).Height = 100;
                    //worksheet.Cells[rowCount, 8].Value = checkPoint.IsForToday ? "" : "Note: Not Applicable for today";
                    rowCount++;
                }
                worksheet.Cells[1, 1, rowCount - 1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1, rowCount - 1, 9].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[1, 1, rowCount - 1, 9].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, rowCount - 1, 9].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, rowCount - 1, 9].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1, rowCount - 1, 9].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                worksheet.Cells[headerColumn + 1, 8, rowCount - 1, 8].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Bottom;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Column(1).Width = 7;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 20;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 20;
                worksheet.Column(8).Width = 20;
                worksheet.Column(9).Width = 40;
                worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = true;
                string headerRange = $"1:{headerColumn}";
                worksheet.PrinterSettings.RepeatRows = worksheet.Cells[headerRange];
                worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                worksheet.PrinterSettings.LeftMargin = 0.25M;
                worksheet.PrinterSettings.RightMargin = 0.25M;
                worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
                worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                worksheet.PrinterSettings.HorizontalCentered = true;
                worksheet.PrinterSettings.FitToPage = true;
                worksheet.PrinterSettings.FitToWidth = 1;
                worksheet.PrinterSettings.FitToHeight = 0;
                worksheet.HeaderFooter.OddFooter.LeftAlignedText = "Date Generated: &D &T";
                worksheet.HeaderFooter.OddFooter.RightAlignedText = "Page: &P";
                worksheet.View.FreezePanes(headerColumn + 1, 1);
                excelPackage.Save();
            }
            var fileName = $"DAIKIN_DigitalCheckSheet_{checkSheet.Name.Replace(" ", "")}_{DateTime.UtcNow.AddMinutes(330).ToString("ddMMMyyyy")}.xlsx";
            memoryStream.Position = 0;
            return (memoryStream, fileName);
        }
        public async Task<IOperationResponse> BulkUpdateCheckRecordAsync(CheckPointBulkEntryDTO checkPointBulkEntryDTO, UserDTO user)
        {
            ValidationContext context = new ValidationContext(checkPointBulkEntryDTO, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(checkPointBulkEntryDTO, context, validationResults, validateAllProperties: true);
            if (isValid)
            {
                if (checkPointBulkEntryDTO.CheckRecord == "NG" || checkPointBulkEntryDTO.CheckRecord == "AbnormalCanUse")
                {
                    return OperationResponse.Error(Common.Util.NgOrAbnormalCanUseRecordNotAllowedForBulkUpdate);
                }

                var checkSheet = await _repository.GetByIdAsync(checkPointBulkEntryDTO.CheckSheetId);

                if (checkSheet == null)
                {
                    return OperationResponse.Error($"Check sheet with id {checkPointBulkEntryDTO.CheckSheetId} not found");
                }

                if (checkSheet.Status == "Approved")
                {
                    return OperationResponse.Error(Common.Util.ApprovedCheckSheetsCannotBeUpdated);
                }

                if (string.IsNullOrEmpty(checkSheet.StartedBy))
                {
                    checkSheet.StartedOn = Util.GetISTLocalDate();
                    checkSheet.StartedBy = user.FullName;
                }

                var settings = ((OperationResponse<MasterSettingsDTO>)(await _masterSettingsService.GetSettingsAsync())).Payload;
                if (checkSheet.LockedBy != null && checkSheet.LockedBy != user.FullName && checkSheet.IsLocked
                    && Util.GetISTLocalDate().Subtract(checkSheet.LockedOn ?? Util.GetISTLocalDate()).TotalMinutes <= settings.Locktime)
                {
                    return OperationResponse.Error($"Check Sheet filling is in-process by {checkSheet.StartedBy}");
                }

                checkSheet.LockedOn = Util.GetISTLocalDate();
                checkSheet.IsLocked = true;
                checkSheet.LockedBy = user.FullName;

                checkSheet.CheckPointTransactions.ToList().ForEach(cp => { cp.CheckRecord = checkPointBulkEntryDTO.CheckRecord; cp.Comments = ""; });

                if (checkSheet.Status == "Not Started")
                {
                    checkSheet.Status = "In Progress";
                }

                if (checkSheet.CheckPointTransactions.Any(x => x.CheckRecord == "NG" || x.CheckRecord == "AbnormalCanUse"))
                {
                    checkSheet.ColorCode = "red";
                    checkSheet.NGRecordExists = true;
                }
                else
                {
                    checkSheet.ColorCode = "green";
                    checkSheet.NGRecordExists = false;
                }

                await _repository.UpdateAsync(checkPointBulkEntryDTO.CheckSheetId, checkSheet);
                var checkRecordCounts = checkSheet.CheckPointTransactions
                                                .Where(c => !string.IsNullOrEmpty(c.CheckRecord))
                                                .GroupBy(c => c.CheckRecord)
                                                .ToDictionary(group => group.Key, group => group.Count());
                return OperationResponse<Dictionary<string, int>>.Success(checkRecordCounts, Common.Util.UpdatedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        public async Task<IOperationResponse> SubmitCheckSheetAsync(string checkSheetId, UserDTO user)
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

            if (checkSheet.Status != "In Progress")
            {
                return OperationResponse.Error(Common.Util.OnlyInProgressCheckSheetsCanBeSubmitted);
            }

            if (checkSheet.CheckPointTransactions.Any(x => string.IsNullOrEmpty(x.CheckRecord)))
            {
                return OperationResponse.Error(Common.Util.PleaseCompleteAllCheckpointsBeforeSubmission);
            }

            if (checkSheet.Status == "Submitted")
            {
                return OperationResponse.Error(Common.Util.CheckSheetAlreadySubmitted);
            }

            var currentTime = Util.GetISTLocalDate().ToString("HH:mm");
            if ((string.Compare(checkSheet.ShiftStartTime, checkSheet.ShiftEndTime) < 0 && string.Compare(checkSheet.ShiftStartTime, currentTime) < 0 && string.Compare(checkSheet.ShiftEndTime, currentTime) > 0) ||
                (string.Compare(checkSheet.ShiftStartTime, checkSheet.ShiftEndTime) > 0 && (string.Compare(checkSheet.ShiftStartTime, currentTime) < 0 || string.Compare(checkSheet.ShiftEndTime, currentTime) > 0)))
            {
                checkSheet.Status = "Submitted";
                checkSheet.SubmittedBy = user.FullName;
                checkSheet.SubmittedOn = Util.GetISTLocalDate();
                await _repository.UpdateAsync(checkSheetId, checkSheet);
            }
            else
            {
                return OperationResponse.Error(Common.Util.ShiftTimingsAreOver);
            }

            var users = ((OperationResponse<IEnumerable<UserDTO>>)await _userService.GetAllAsync()).Payload;
            var emailToList = users.Where(x => x.Role == "Validator" && x.DepartmentId == checkSheet.DepartmentId && x.LineIds.Contains(checkSheet.LineId) && !string.IsNullOrEmpty(x.Email)).Select(x => x.Email).Distinct();
            var emailCCList = users.Where(x => x.Role == "SuperAdmin" && !string.IsNullOrWhiteSpace(x.Email)).Select(x => x.Email).Distinct();

            if (!emailToList.Any()) emailToList = emailCCList;
            if (emailToList.Any())
            {
                var subject = $"[{_options.Value.Env}] Digital Check Sheet - {checkSheet.Name} - Submitted for Validation";
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
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Started By</td>");
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.StartedBy + "</td>");
                htmlTable.Append("</tr>");

                htmlTable.Append("<tr>");
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Started On</td>");
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + Common.Util.GetDateLongText(checkSheet.StartedOn ?? Util.GetISTLocalDate()) + "</td>");
                htmlTable.Append("</tr>");

                htmlTable.Append("<tr>");
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Submitted By</td>");
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.SubmittedBy + "</td>");
                htmlTable.Append("</tr>");

                htmlTable.Append("<tr>");
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Submitted On</td>");
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + Common.Util.GetDateLongText(checkSheet.SubmittedOn ?? Util.GetISTLocalDate()) + "</td>");
                htmlTable.Append("</tr>");

                htmlTable.Append("<tr>");
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>Shift</td>");
                htmlTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkSheet.Shift + "</td>");
                htmlTable.Append("</tr>");

                htmlTable.Append("</table>");

                var checkPointTable = new StringBuilder();

                if (checkSheet.CheckPointTransactions.Where(x => x.CheckRecord == "NG" || x.CheckRecord == "AbnormalCanUse").Any())
                {
                    checkPointTable.Append("<table border='1'>");
                    checkPointTable.Append("<tr style='background-color: #ccc; padding: 10px; border: 1px solid #ddd;'><th>Sl.No.</th><th>Check Point</th><th>Check Record</th><th>Comment</th></tr>");
                    foreach (var checkPoint in checkSheet.CheckPointTransactions.Where(x => x.CheckRecord == "NG" || x.CheckRecord == "AbnormalCanUse"))
                    {
                        checkPointTable.Append("<tr>");
                        checkPointTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkPoint.SeqOrder + "</td>");
                        checkPointTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkPoint.Name + "</td>");
                        checkPointTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkPoint.CheckRecord + "</td>");
                        checkPointTable.Append("<td style='text-align: left; padding: 10px; border: 1px solid #ddd;'>" + checkPoint.Comments + "</td>");
                        checkPointTable.Append("</tr>");
                    }
                    checkPointTable.Append("</table>");
                }
                else
                {
                    checkPointTable.Append("<b>Note:</b> There are no NG or Abnormal (can use) checkpoints found");
                }

                var badgeCount = checkSheet.CheckPointTransactions
                                                    .Where(c => !string.IsNullOrEmpty(c.CheckRecord))
                                                    .GroupBy(c => c.CheckRecord)
                                                    .ToDictionary(group => group.Key, group => group.Count());
                string notes = "<b>Check Point Summary: </b>" + string.Join(", ", badgeCount.Select(pair => $"{pair.Key} Count: {pair.Value}"));

                var body = File.ReadAllText("DailyCheckSheet_WorkFlowTemplate.txt")
                               .Replace("@CheckSheet@", htmlTable.ToString())
                               .Replace("@CheckPoints@", checkPointTable.ToString())
                               .Replace("@Notes@", notes)
                               .Replace("@URL@", _options.Value.URL);
                var checkSheetEmail = new CheckSheetEmail { Subject = subject, Body = body, Env = _options.Value.Env, ToList = string.Join(",", emailToList), CcList = string.Join(",", emailCCList), CreatedBy = user.FullName, CheckSheetTranactionId = checkSheetId };
                await _emailRepository.CreateAsync(checkSheetEmail);
            }
            else
            {
                _logger.LogError("There are no TO email list found");
            }
            return OperationResponse.Success(Common.Util.CheckSheetSubmittedSuccessfully);
        }
        public async Task<IOperationResponse> SendCheckSheetEmails(string api_key)
        {
            if (api_key != _options.Value.ApiKey)
            {
                return OperationResponse.Error(Common.Util.ApiKeyIncorrect);
            }
            var emails = await _emailRepository.GetByFilterAsync("EmailSent", false);
            var currentTime = Util.GetISTLocalDate();
            var timeSpan = TimeSpan.FromMinutes(30);
            var emailCount = 0;

            foreach (var email in emails.Where(email => currentTime - email.CreatedOn <= timeSpan).OrderBy(x => x.CreatedOn))
            {
                var existingEmal = await _emailRepository.GetByIdAsync(email.Id);
                if (!existingEmal.EmailSent)
                {
                    email.EmailSent = true;
                    email.ModifiedOn = Common.Util.GetISTLocalDate();
                    await _emailRepository.UpdateAsync(email.Id, email);

                    var (isEmailSuccess, errorMessage) = SendMail(email.Subject, email.Body, email.Env, email.ToList, email.CcList);

                    email.ErrorMessage = errorMessage;
                    email.EmailDelivered = isEmailSuccess;
                    email.EmailSentOn = Common.Util.GetISTLocalDate();
                    await _emailRepository.UpdateAsync(email.Id, email);
                    emailCount++;
                }
            }
            return OperationResponse.Success($"{emailCount} out of {emails.Count()} emails sent");
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

            if (checkSheet.Status != "Submitted")
            {
                return OperationResponse.Error(Common.Util.OnlySubmittedCheckSheetsCanBeApproved);
            }

            if (checkSheet.CheckPointTransactions.Any(x => string.IsNullOrEmpty(x.CheckRecord)))
            {
                return OperationResponse.Error(Common.Util.PleaseCompleteAllCheckpointsBeforeSubmission);
            }

            checkSheet.Status = "Approved";
            checkSheet.ValidatedBy = user.FullName;
            checkSheet.ValidatedOn = Util.GetISTLocalDate();
            await _repository.UpdateAsync(checkSheetId, checkSheet);
            return OperationResponse.Success(Common.Util.CheckSheetApprovedSuccessfully);
        }
        public async Task<IOperationResponse> UpdateCheckRecordAsync(CheckPointEntryDTO checkPointEntryDTO, UserDTO user)
        {
            ValidationContext context = new ValidationContext(checkPointEntryDTO, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(checkPointEntryDTO, context, validationResults, validateAllProperties: true);
            if (isValid)
            {

                if ((checkPointEntryDTO.CheckRecord == "NG" || checkPointEntryDTO.CheckRecord == "AbnormalCanUse")
                    && string.IsNullOrWhiteSpace(checkPointEntryDTO.Comments))
                {
                    return OperationResponse.Error(Common.Util.CommentsCannotBeEmptyForNg);
                }

                var checkSheet = await _repository.GetByIdAsync(checkPointEntryDTO.CheckSheetId);

                if (checkSheet == null)
                {
                    return OperationResponse.Error($"Check sheet with id {checkPointEntryDTO.CheckSheetId} not found");
                }

                if (checkSheet.Status == "Approved")
                {
                    return OperationResponse.Error(Common.Util.ApprovedCheckSheetsCannotBeUpdated);
                }

                if (string.IsNullOrEmpty(checkSheet.StartedBy))
                {
                    checkSheet.StartedOn = Util.GetISTLocalDate();
                    checkSheet.StartedBy = user.FullName;
                }

                var settings = ((OperationResponse<MasterSettingsDTO>)(await _masterSettingsService.GetSettingsAsync())).Payload;
                if (checkSheet.LockedBy != null && checkSheet.LockedBy != user.FullName && checkSheet.IsLocked
                    && Util.GetISTLocalDate().Subtract(checkSheet.LockedOn ?? Util.GetISTLocalDate()).TotalMinutes <= settings.Locktime)
                {
                    return OperationResponse.Error($"Check Sheet filling is in-process by {checkSheet.StartedBy}");
                }

                checkSheet.LockedOn = Util.GetISTLocalDate();
                checkSheet.IsLocked = true;
                checkSheet.LockedBy = user.FullName;
                if (checkSheet.Status == "Not Started")
                {
                    checkSheet.Status = "In Progress";
                }

                var checkPoint = checkSheet.CheckPointTransactions.FirstOrDefault(x => x.Id == checkPointEntryDTO.CheckPointId);

                if (checkPoint == null)
                {
                    return OperationResponse.Error($"Check record with id {checkPointEntryDTO.CheckPointId} not found");
                }

                checkPoint.CheckRecord = checkPointEntryDTO.CheckRecord;
                checkPoint.Comments = checkPointEntryDTO.Comments;

                if ((checkPointEntryDTO.UserAction == "Submitted" || checkPointEntryDTO.UserAction == "Approved") &&
                    checkSheet.CheckPointTransactions.Any(x => string.IsNullOrEmpty(x.CheckRecord)))
                {
                    return OperationResponse.Error(Common.Util.PleaseCompleteAllCheckpointsBeforeSubmission);
                }

                if (checkSheet.CheckPointTransactions.Any(x => x.CheckRecord == "NG" || x.CheckRecord == "AbnormalCanUse"))
                {
                    checkSheet.ColorCode = "red";
                    checkSheet.NGRecordExists = true;
                }
                else
                {
                    checkSheet.ColorCode = "green";
                    checkSheet.NGRecordExists = false;
                }
                await _repository.UpdateAsync(checkPointEntryDTO.CheckSheetId, checkSheet);
                var checkRecordCounts = checkSheet.CheckPointTransactions
                                                .Where(c => !string.IsNullOrEmpty(c.CheckRecord))
                                                .GroupBy(c => c.CheckRecord)
                                                .ToDictionary(group => group.Key, group => group.Count());

                return OperationResponse<Dictionary<string, int>>.Success(checkRecordCounts, Common.Util.CheckRecordUpdatedSuccessfully);
            }
            else
            {
                return OperationResponse.Error(string.Join("\n", validationResults.Select(x => x.ErrorMessage)));
            }
        }
        private (bool, string) SendMail(string subject, string body, string env, string emailTo, string emailCC)
        {
            var settings = ((OperationResponse<MasterSettingsDTO>)(_masterSettingsService.GetSettingsAsync().Result)).Payload;
            int maxRetries = 3;
            int retryDelay = 5000;
            bool sentSuccessfully = false;
            int retries = 0;
            string errorMessage = "";

            while (!sentSuccessfully && retries < maxRetries)
            {
                try
                {
                    var message = new MailMessage();
                    var smtpClient = new SmtpClient();
                    var userid = settings.SMTPUserId;
                    var password = settings.SMTPPassword;
                    var fromAddress = new MailAddress(settings.SenderEmailAddress, $"[{env}] Digital Check Sheet");
                    message.From = fromAddress;
                    message.To.Add(emailTo);
                    message.CC.Add(emailCC);
                    message.Subject = subject;
                    message.IsBodyHtml = true;
                    message.Body = body;
                    smtpClient.Host = settings.SMTPHost;
                    smtpClient.Port = settings.SMTPPort;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential(userid, password);
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = settings.SMTPEnableSSL;
                    smtpClient.Timeout = 30000;
                    smtpClient.Send(message);
                    sentSuccessfully = true;
                }
                catch (SmtpException ex)
                {
                    errorMessage = $"{ex.Message} - {retries}/{maxRetries}";
                    if (IsSMTPException(ex) && retries < maxRetries - 1)
                    {
                        retries++;
                        _logger.LogInformation($"SendMail: {ex.Message}. Retrying attempt {retries + 1} in {retryDelay / 1000} seconds...");
                        System.Threading.Thread.Sleep(retryDelay);
                    }
                    else
                    {
                        retries = 5;
                        _logger.LogError(ex, $"SendMail - TO: {emailTo} CC: {emailCC}");
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    _logger.LogError($"SendMail: {ex.Message}", ex);
                }
            }
            return (sentSuccessfully, errorMessage);
        }
        private bool IsSMTPException(SmtpException ex)
        {
            return ex.Message.Contains("timed out") || ex.Message.Contains("Name or service");
        }
        public async Task<IOperationResponse> ArchiveAsync(string api_key)
        {
            if (api_key != _options.Value.ApiKey)
            {
                return OperationResponse.Error(Common.Util.ApiKeyIncorrect);
            }
            var recordCount = await _repository.ArchiveOldTransactionsAsync();

            var emalRecordCount = await _repository.DeleteOldEmailsAsync();

            return OperationResponse.Success($"{recordCount} transactions archived and {emalRecordCount} emails deleted successfully");
        }
        public async Task<IOperationResponse> ArchiveInitialRunAsync(string api_key)
        {
            if (api_key != _options.Value.ApiKey)
            {
                return OperationResponse.Error(Common.Util.ApiKeyIncorrect);
            }
            var recordCount = await _repository.ArchiveOldTransactionsInitialRunAsync();

            return OperationResponse.Success($"{recordCount} archived successfully");
        }
    }
}
