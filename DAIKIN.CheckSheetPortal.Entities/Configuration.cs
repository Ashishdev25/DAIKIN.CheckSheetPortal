namespace DAIKIN.CheckSheetPortal.Entities
{
    public class Configuration: BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool CanDelete { get; set; } = true;   
    }
}
