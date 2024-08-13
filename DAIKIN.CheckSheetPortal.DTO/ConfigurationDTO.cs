using System.ComponentModel.DataAnnotations;

namespace DAIKIN.CheckSheetPortal.DTO
{
    public class ConfigurationDTO
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Name must be a valid value.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Code must be a valid value.")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Description must be a valid value.")]
        public string Description { get; set; }
    }
}