using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.DTO
{
    public class MasterSettingsDTO : BaseEntityDTO
    {
        [Required(ErrorMessage = "Locktime is required")]
        public int Locktime { get; set; }
        public IEnumerable<ShiftDTO> Shifts { get; set; }
        [Required(ErrorMessage = "SenderEmailAddress is required")]
        public string SenderEmailAddress { get; set; }
        public bool SMTPEnableSSL { get; set; }
        [Required(ErrorMessage = "SMTPHost is required")]
        public string SMTPHost { get; set; }
        [Required(ErrorMessage = "SMTPPort is required")]
        public int SMTPPort { get; set; }
        [Required(ErrorMessage = "SMTPUserId is required")]
        public string SMTPUserId { get; set; }
        [Required(ErrorMessage = "SMTPPassword is required")]
        public string SMTPPassword { get; set; }
    }
    public class ShiftDTO
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "StartTime is required")]
        public string StartTime { get; set; }
        [Required(ErrorMessage = "EndTime is required")]
        public string EndTime { get; set; }
    }
}
