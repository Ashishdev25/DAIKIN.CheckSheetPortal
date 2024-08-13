using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.Entities
{
    public class CheckSheetEmail : BaseEntity
    {
        public string CheckSheetTranactionId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Env { get; set; }
        public string ToList { get; set; }
        public string CcList { get; set; }
        public bool EmailSent { get; set; } = false;
        public bool EmailDelivered { get; set; } = false;
        public string ErrorMessage { get; set; }
        public DateTime EmailSentOn { get; set; }
    }
}
