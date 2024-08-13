namespace DAIKIN.CheckSheetPortal.Entities
{
    public class CheckSheetTransactionArchive: CheckSheetTransaction
    {

    }
    public class CheckSheetTransaction: CheckSheet
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
        public string? SubmittedBy { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public string? ValidatedBy { get; set; }
        public DateTime? ValidatedOn { get; set; }
        public string LockedBy { get; set; }
        public string Status { get; set; } = "Not Started";
        public string ColorCode { get; set; } = "black";
        public bool NGRecordExists { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public DateTime? LockedOn { get; set; }
        public IEnumerable<CheckPointTransaction> CheckPointTransactions { get; set; }
    }
    public class CheckPointTransaction : CheckPoint
    {
        public string CheckRecord { get; set; }
        public string Comments { get; set; }
        public bool IsForToday { get; set; }
    }
}
