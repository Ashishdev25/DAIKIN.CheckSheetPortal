using DAIKIN.CheckSheetPortal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.Infrastructure.DataAccess
{
    public interface ICheckSheetVersionRepository : IRepository<CheckSheetVersion>
    {
        Task<IEnumerable<CheckSheetVersion>> GetLatestVersionsAsync();
    }
}
