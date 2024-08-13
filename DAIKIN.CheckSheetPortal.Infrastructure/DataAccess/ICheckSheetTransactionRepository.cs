using DAIKIN.CheckSheetPortal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAIKIN.CheckSheetPortal.Infrastructure.DataAccess
{
    public interface ICheckSheetTransactionRepository : IRepository<CheckSheetTransaction>
    {
        Task<int> ArchiveOldTransactionsAsync();
        Task<int> ArchiveOldTransactionsInitialRunAsync();
        Task<int> DeleteOldEmailsAsync();
    }
}
