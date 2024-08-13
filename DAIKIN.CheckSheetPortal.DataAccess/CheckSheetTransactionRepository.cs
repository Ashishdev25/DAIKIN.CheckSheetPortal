using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure.DataAccess;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace DAIKIN.CheckSheetPortal.DataAccess
{
    public class CheckSheetTransactionRepository : Repository<CheckSheetTransaction>, ICheckSheetTransactionRepository
    {
        private readonly IMongoCollection<CheckSheetTransaction> _collection;
        private readonly IMongoCollection<CheckSheetTransactionArchive> _archiveCollection;
        private readonly IMongoCollection<CheckSheetEmail> _emailCollection;
        public CheckSheetTransactionRepository(string connectionString, string databaseName, string collectionName) : base(connectionString, databaseName, collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<CheckSheetTransaction>(collectionName);
            _archiveCollection = database.GetCollection<CheckSheetTransactionArchive>("check_sheet_transactions_archive");
            _emailCollection = database.GetCollection<CheckSheetEmail>("check_sheet_emails");
        }
        public async Task<int> ArchiveOldTransactionsInitialRunAsync()
        {
            // Get the minimum CheckSheetDay from the collection
            var minCheckSheetDay = await _collection.Find(_ => true)
                                                     .SortBy(c => c.CheckSheetDay)
                                                     .Limit(1)
                                                     .Project(c => c.CheckSheetDay)
                                                     .FirstOrDefaultAsync();

            if (minCheckSheetDay == default)
            {
                return 0; // No records found
            }

            int totalArchivedRecords = 0;
            var currentDate = DateTime.UtcNow.Date;

            for (var date = minCheckSheetDay.Date; date < currentDate; date = date.AddDays(1))
            {
                var nextDay = date.AddDays(1);
                var filter = Builders<CheckSheetTransaction>.Filter.And(
                    Builders<CheckSheetTransaction>.Filter.Gte(c => c.CheckSheetDay, date),
                    Builders<CheckSheetTransaction>.Filter.Lt(c => c.CheckSheetDay, nextDay)
                );

                var transactionsToArchive = await _collection.Find(filter).ToListAsync();

                if (transactionsToArchive.Any())
                {
                    var archiveTransactions = transactionsToArchive.Select(t => new CheckSheetTransactionArchive
                    {
                        ChangeDetails = t.ChangeDetails,
                        CheckPointTransactions = t.CheckPointTransactions.Select(cp => new CheckPointTransaction
                        {
                            CheckRecord = cp.CheckRecord,
                            Comments = cp.Comments,
                            IsForToday = cp.IsForToday,
                            CompleteInSeconds = cp.CompleteInSeconds,
                            Condition = cp.Condition,
                            CreatedBy = cp.CreatedBy,
                            CreatedOn = cp.CreatedOn,
                            FileName = cp.FileName,
                            FrequencyText = cp.FrequencyText,
                            FrequencyType = cp.FrequencyType,
                            Id = cp.Id,
                            IsActive = cp.IsActive,
                            Method = cp.Method,
                            ModifiedBy = cp.ModifiedBy,
                            ModifiedOn = cp.ModifiedOn,
                            MonthDays = cp.MonthDays,
                            Name = cp.Name,
                            SeqOrder = cp.SeqOrder,
                            Standard = cp.Standard,
                            UniqueFileName = cp.UniqueFileName,
                            WeekDays = cp.WeekDays,
                            YearlyMonthDays = cp.YearlyMonthDays,
                            YearlyMonths = cp.YearlyMonths
                        }).ToList(),
                        CreatedBy = t.CreatedBy,
                        CreatedOn = t.CreatedOn,
                        DepartmentId = t.DepartmentId,
                        EquipmentId = t.EquipmentId,
                        Id = t.Id,
                        IsActive = t.IsActive,
                        LineId = t.LineId,
                        LocationId = t.LocationId,
                        MaintenaceClassId = t.MaintenaceClassId,
                        ModifiedBy = t.ModifiedBy,
                        ModifiedOn = t.ModifiedOn,
                        Name = t.Name,
                        Revision = t.Revision,
                        Shift = t.Shift,
                        ShiftEndTime = t.ShiftEndTime,
                        ShiftStartTime = t.ShiftStartTime,
                        StationId = t.StationId,
                        SubLocationId = t.SubLocationId,
                        UniqueId = t.UniqueId,
                        Version = t.Version,
                        CheckSheetId = t.CheckSheetId,
                        Department = t.Department,
                        Line = t.Line,
                        MaintenaceClass = t.MaintenaceClass,
                        Station = t.Station,
                        Equipment = t.Equipment,
                        EquipmentCode = t.EquipmentCode,
                        Location = t.Location,
                        SubLocation = t.SubLocation,
                        CheckSheetDay = t.CheckSheetDay,
                        StartedBy = t.StartedBy,
                        StartedOn = t.StartedOn,
                        ValidatedBy = t.ValidatedBy,
                        ValidatedOn = t.ValidatedOn,
                        LockedBy = t.LockedBy,
                        Status = t.Status,
                        ColorCode = t.ColorCode,
                        NGRecordExists = t.NGRecordExists,
                        IsLocked = t.IsLocked,
                        LockedOn = t.LockedOn,
                        SubmittedOn = t.SubmittedOn,
                        SubmittedBy = t.SubmittedBy,
                    }).ToList();

                    await _archiveCollection.InsertManyAsync(archiveTransactions);

                    var idsToRemove = transactionsToArchive.Select(t => t.Id).ToList();
                    var deleteFilter = Builders<CheckSheetTransaction>.Filter.In(t => t.Id, idsToRemove);
                    await _collection.DeleteManyAsync(deleteFilter);

                    totalArchivedRecords += idsToRemove.Count();
                }
            }

            return totalArchivedRecords;
        }

        public async Task<int> ArchiveOldTransactionsAsync()
        {
            var filter = Builders<CheckSheetTransaction>.Filter.Lt(c => c.CheckSheetDay, DateTime.UtcNow.AddDays(-1));
            var transactionsToArchive = await _collection.Find(filter).ToListAsync();
            var recordCount = 0;

            if (transactionsToArchive.Any())
            {
                var archiveTransactions = transactionsToArchive.Select(t => new CheckSheetTransactionArchive
                {
                    ChangeDetails = t.ChangeDetails,
                    CheckPointTransactions = t.CheckPointTransactions.Select(cp => new CheckPointTransaction
                    {
                        CheckRecord = cp.CheckRecord,
                        Comments = cp.Comments,
                        IsForToday = cp.IsForToday,
                        CompleteInSeconds = cp.CompleteInSeconds,
                        Condition = cp.Condition,
                        CreatedBy = cp.CreatedBy,
                        CreatedOn = cp.CreatedOn,
                        FileName = cp.FileName,
                        FrequencyText = cp.FrequencyText,
                        FrequencyType = cp.FrequencyType,
                        Id = cp.Id,
                        IsActive = cp.IsActive,
                        Method = cp.Method,
                        ModifiedBy = cp.ModifiedBy,
                        ModifiedOn = cp.ModifiedOn,
                        MonthDays = cp.MonthDays,
                        Name = cp.Name,
                        SeqOrder = cp.SeqOrder,
                        Standard = cp.Standard,
                        UniqueFileName = cp.UniqueFileName,
                        WeekDays = cp.WeekDays,
                        YearlyMonthDays = cp.YearlyMonthDays,
                        YearlyMonths = cp.YearlyMonths
                    }).ToList(),
                    CreatedBy = t.CreatedBy,
                    CreatedOn = t.CreatedOn,
                    DepartmentId = t.DepartmentId,
                    EquipmentId = t.EquipmentId,
                    Id = t.Id,
                    IsActive = t.IsActive,
                    LineId = t.LineId,
                    LocationId = t.LocationId,
                    MaintenaceClassId = t.MaintenaceClassId,
                    ModifiedBy = t.ModifiedBy,
                    ModifiedOn = t.ModifiedOn,
                    Name = t.Name,
                    Revision = t.Revision,
                    Shift = t.Shift,
                    ShiftEndTime = t.ShiftEndTime,
                    ShiftStartTime = t.ShiftStartTime,
                    StationId = t.StationId,
                    SubLocationId = t.SubLocationId,
                    UniqueId = t.UniqueId,
                    Version = t.Version,
                    CheckSheetId = t.CheckSheetId,
                    Department = t.Department,
                    Line = t.Line,
                    MaintenaceClass = t.MaintenaceClass,
                    Station = t.Station,
                    Equipment = t.Equipment,
                    EquipmentCode = t.EquipmentCode,
                    Location = t.Location,
                    SubLocation = t.SubLocation,
                    CheckSheetDay = t.CheckSheetDay,
                    StartedBy = t.StartedBy,
                    StartedOn = t.StartedOn,
                    ValidatedBy = t.ValidatedBy,
                    ValidatedOn = t.ValidatedOn,
                    LockedBy = t.LockedBy,
                    Status = t.Status,
                    ColorCode = t.ColorCode,
                    NGRecordExists = t.NGRecordExists,
                    IsLocked = t.IsLocked,
                    LockedOn = t.LockedOn
                }).ToList();

                await _archiveCollection.InsertManyAsync(archiveTransactions);

                var idsToRemove = transactionsToArchive.Select(t => t.Id).ToList();
                var deleteFilter = Builders<CheckSheetTransaction>.Filter.In(t => t.Id, idsToRemove);
                await _collection.DeleteManyAsync(deleteFilter);
                recordCount = idsToRemove.Count();
            }
            return recordCount;
        }
        public async Task<int> DeleteOldEmailsAsync()
        {
            var filter = Builders<CheckSheetEmail>.Filter.Lt(c => c.CreatedOn, DateTime.UtcNow.AddDays(-1));
            var emailsToDelete = await _emailCollection.Find(filter).ToListAsync();
            var recordCount = 0;

            var idsToRemove = emailsToDelete.Select(t => t.Id).ToList();
            if (idsToRemove.Count > 0)
            {
                var deleteFilter = Builders<CheckSheetEmail>.Filter.In(t => t.Id, idsToRemove);
                await _emailCollection.DeleteManyAsync(deleteFilter);
                recordCount = idsToRemove.Count();
            }

            return recordCount;
        }
    }
}
