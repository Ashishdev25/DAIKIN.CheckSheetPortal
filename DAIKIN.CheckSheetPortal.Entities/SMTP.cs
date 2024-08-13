using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.Entities
{
    public class SMTP
    {
        public string SenderEmailAddpress { get; set; }
        public bool EnableSSL { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
    }
}
