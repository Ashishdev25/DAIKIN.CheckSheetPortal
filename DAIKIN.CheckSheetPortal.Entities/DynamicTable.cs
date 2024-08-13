namespace DAIKIN.CheckSheetPortal.Entities
{
    public class DynamicTable
    {
        public string TableName { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }
        public string GlobalSearch { get; set; }
        public bool IncludeVersion { get; set; }
        public List<DynamicFilter> DynamicFilter { get; set; }
    }
}