namespace DAIKIN.CheckSheetPortal.Entities
{
    public class CheckSheet: BaseEntity
    {       
        public string Name { get; set; }
        public string LineId { get; set; }
        public string DepartmentId { get; set; }
        public string UniqueId { get; set; }
        public string EquipmentId { get; set; }
        public string MaintenaceClassId { get; set; }
        public string StationId { get; set; }
        public string LocationId { get; set; }
        public string SubLocationId { get; set; }
        public int Version { get; set; } = 1;
        public string Shift { get; set; }
        public string ShiftStartTime { get; set; }
        public string ShiftEndTime { get; set; }
        public string Revision { get; set; } = "New";
        public string ChangeDetails { get; set; } = "Initial Version";
        public List<CheckPoint> CheckPoints { get; set; }
    }
    public class CheckPoint : BaseEntity
    {
        public string Name { get; set; }
        public string Standard { get; set; }
        public string Condition { get; set; }
        public string Method { get; set; }
        public int CompleteInSeconds { get; set; }
        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
        public int SeqOrder { get; set; }
        public string FrequencyType { get; set; }
        public string FrequencyText { get; set; }
        public List<int> WeekDays { get; set; }
        public List<int> MonthDays { get; set; }
        public List<int> YearlyMonths { get; set; }
        public List<int> YearlyMonthDays { get; set; }
    }
}
