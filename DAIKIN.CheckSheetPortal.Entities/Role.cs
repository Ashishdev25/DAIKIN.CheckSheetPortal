namespace DAIKIN.CheckSheetPortal.Entities
{
    public class Role: BaseEntity
    {
        public string Name { get; set; }
        public bool CanDelete { get; set; }
    }
}
