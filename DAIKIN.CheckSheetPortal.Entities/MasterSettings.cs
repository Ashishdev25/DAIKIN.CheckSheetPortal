namespace DAIKIN.CheckSheetPortal.Entities
{
    public class MasterSettings: BaseEntity
    {
        public int Locktime { get; set; }
        public IEnumerable<Shift> Shifts { get; set; }
        public string SenderEmailAddress { get; set; }
        public bool SMTPEnableSSL { get; set; }
        public string SMTPHost { get; set; }
        public int SMTPPort { get; set; }
        public string SMTPUserId { get; set; }
        public string SMTPPassword { get; set; }
    }
    public class Shift
    {
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
