using System.ComponentModel.DataAnnotations;

namespace DAIKIN.CheckSheetPortal.DTO
{
    public class UserDTO : BaseEntityDTO
    {
        public string Id { get; set; }
        public string LoginId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool CanDelete { get; set; }
        public string DepartmentId { get; set; }
        public string Department { get; set; }
        public string Plant { get; set; }
        public string PlantId { get; set; }
        public List<string> LineIds { get; set; }
        public List<string> Lines { get; set; }
    }
}