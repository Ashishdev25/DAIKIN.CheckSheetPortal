using System.ComponentModel.DataAnnotations;

namespace DAIKIN.CheckSheetPortal.DTO
{
    public class CheckSheetTransactionMaster
    {
        public Dictionary<string, int> BadgeCount { get; set; }
        public IEnumerable<CheckSheetTransactionDTO> CheckSheetTransaction { get; set; }
    }
    public class CheckSheetTransactionDTO : CheckSheetDTO
    {
        public string CheckSheetId { get; set; }
        public string Department { get; set; }
        public string Line { get; set; }
        public string MaintenaceClass { get; set; }
        public string Station { get; set; }
        public string Equipment { get; set; }
        public string EquipmentCode { get; set; }
        public string Location { get; set; }
        public string SubLocation { get; set; }
        public DateTime CheckSheetDay { get; set; }
        public string? StartedBy { get; set; }
        public DateTime? StartedOn { get; set; }
        public string? ValidatedBy { get; set; }
        public DateTime? ValidatedOn { get; set; }
        public string Status { get; set; } = "Not Started";
        public string ColorCode { get; set; } = "black";
        public bool NGRecordExists { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public DateTime? LockedOn { get; set; }
    }
    public class CheckSheetTransactionFullMaster
    {
        public Dictionary<string, int> BadgeCount { get; set; }
        public UserActionsDTO UserActions { get; set; }
        public CheckSheetTransactionFullDTO CheckSheetTransaction { get; set; }
    }
    public class UserActionsDTO
    {
        public bool IsReadOnly { get; set; }
        public bool ShowExportPrintVersion { get; set; }
        public bool ShowSubmitButton { get; set; }
        public bool ShowApproveButton { get; set; }
    }   
    public class CheckSheetTransactionFullDTO : CheckSheetTransactionDTO
    {
        public IEnumerable<CheckPointTransactionDTO> CheckPointTransactions { get; set; }
    }
    public class CheckPointTransactionDTO : CheckPointDTO
    {
        public string CheckRecord { get; set; }
        public string Comments { get; set; }
        public bool IsForToday { get; set; }
    }
    public class CheckPointBulkEntryDTO
    {
        [Required(ErrorMessage = "CheckSheetId must be a valid value.")]
        public string CheckSheetId { get; set; }

        [RegularExpression("^(OK|NG|AbnormalCanUse|NA)$", ErrorMessage = "Action must be 'OK', 'NG', 'AbnormalCanUse', or 'NA'.")]
        public string CheckRecord { get; set; }

        [RegularExpression("^(Save|Submit|Reviewed|Approved)$", ErrorMessage = "Action must be 'Save', 'Submit', 'Reviewed', or 'Approved'.")]
        public string UserAction { get; set; }
    }
    public class CheckPointEntryDTO : CheckPointBulkEntryDTO
    {
        [Required(ErrorMessage = "CheckPointId must be a valid value.")]
        public string CheckPointId { get; set; }
        public string Comments { get; set; }
    }
    public class ViewCheckSheetTransactionDTO
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public string Line { get; set; }
        public string MaintenaceClass { get; set; }
        public string Station { get; set; }
        public string Equipment { get; set; }
        public string EquipmentCode { get; set; }
        public string Location { get; set; }
        public string SubLocation { get; set; }
        public DateTime CheckSheetDay { get; set; }
        public string? StartedBy { get; set; }
        public DateTime? StartedOn { get; set; }
        public string? ValidatedBy { get; set; }
        public DateTime? ValidatedOn { get; set; }
        public string Status { get; set; }
        public string Shift { get; set; }
        public bool NGRecordExists { get; set; }
        public string Revision { get; set; } 
        public string ChangeDetails { get; set; }
    }
}
