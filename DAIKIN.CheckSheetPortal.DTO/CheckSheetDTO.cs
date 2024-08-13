using System.ComponentModel.DataAnnotations;

namespace DAIKIN.CheckSheetPortal.DTO
{
    public class CheckSheetDTO : BaseEntityDTO
    {
        [Required(ErrorMessage = "Name must be a valid value.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "LineId must be a valid value.")]
        public string LineId { get; set; }
        [Required(ErrorMessage = "DepartmentId must be a valid value.")]
        public string DepartmentId { get; set; }
        [Required(ErrorMessage = "UniqueId must be a valid value.")]
        public string UniqueId { get; set; }
        [Required(ErrorMessage = "EquipmentId must be a valid value.")]
        public string EquipmentId { get; set; }
        [Required(ErrorMessage = "MaintenaceClassId must be a valid value.")]
        public string MaintenaceClassId { get; set; }
        [Required(ErrorMessage = "StationId must be a valid value.")]
        public string StationId { get; set; }
        [Required(ErrorMessage = "LocationId must be a valid value.")]
        public string LocationId { get; set; }
        [Required(ErrorMessage = "SubLocationId must be a valid value.")]
        public string SubLocationId { get; set; }
        public int Version { get; set; } = 1;
        public string Shift { get; set; }
        public string ShiftStartTime { get; set; }
        public string ShiftEndTime { get; set; }
        public string Revision { get; set; } = "New";
        public string ChangeDetails { get; set; } = "Initial Version";
    }
    public class CheckSheetFullDTO : CheckSheetDTO
    {
        public List<CheckPointDTO> CheckPoints { get; set; }
    }
    public class CheckPointDTO
    {
        [Required(ErrorMessage = "Name must be a valid value.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Standard must be a valid value.")]
        public string Standard { get; set; }
        [Required(ErrorMessage = "Condition must be a valid value.")]
        public string Condition { get; set; }
        [Required(ErrorMessage = "Method must be a valid value.")]
        public string Method { get; set; }
        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
        [Required(ErrorMessage = "SeqOrder must be a valid value.")]
        [Range(1, 60, ErrorMessage = "SeqOrder must be greater than 0.")]
        public int SeqOrder { get; set; }
        public string Id { get; set; }
        [Required(ErrorMessage = "FrequencyType must be a valid value.")]
        [RegularExpression("^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
        public string FrequencyType { get; set; }
        [Required(ErrorMessage = "CompleteInSeconds must be a valid value.")]
        [Range(1, 60, ErrorMessage = "CompleteInSeconds must be greater than 0.")]
        public int CompleteInSeconds { get; set; }
        public string FrequencyText { get; set; }
        public List<int> WeekDays { get; set; }
        public List<int> MonthDays { get; set; }
        public List<int> YearlyMonths { get; set; }
        public List<int> YearlyMonthDays { get; set; }
    }
}
