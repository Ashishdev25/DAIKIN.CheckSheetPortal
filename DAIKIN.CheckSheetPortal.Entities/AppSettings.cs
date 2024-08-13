using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.Entities
{
    public class AppSettings
    {
        public string Env { get; set; }
        public string FolderPath { get; set; }
        public string URL { get; set; }
        public string ApiKey { get; set; }
    }
}
