using AutoMapper;
using DAIKIN.CheckSheetPortal.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DAIKIN.CheckSheetPortal.DataAccess
{
    public class StartupTableCreator
    {
        private readonly IMongoCollection<Configuration> _configCollection;
        private readonly IMongoCollection<CheckSheet> _checkSheetCollection;
        private readonly IMongoCollection<CheckSheetVersion> _checkSheetVersionCollection;
        private readonly IMongoCollection<CheckSheetTransaction> _checkSheetTransactionCollection;
        private readonly IMongoCollection<CheckSheetTransactionArchive> _checkSheetTransactionArchiveCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Role> _roleCollection;
        private readonly IMongoCollection<MasterSettings> _settingsCollection;
        private readonly IMongoDatabase _database;
        private readonly IMapper _mapper;
        private readonly string _env;
        public StartupTableCreator(IMongoDatabase database, string env, IMapper mapper)
        {
            _configCollection = database.GetCollection<Configuration>("configuration");
            _checkSheetCollection = database.GetCollection<CheckSheet>("check_sheets");
            _checkSheetVersionCollection = database.GetCollection<CheckSheetVersion>("check_sheet_versions");
            _checkSheetTransactionCollection = database.GetCollection<CheckSheetTransaction>("check_sheet_transactions");
            _checkSheetTransactionArchiveCollection = database.GetCollection<CheckSheetTransactionArchive>("check_sheet_transactions_archive");
            _userCollection = database.GetCollection<User>("users");
            _settingsCollection = database.GetCollection<MasterSettings>("settings");
            _env = env.ToLower();
            _mapper = mapper;
            _database = database;
        }
        public void CreateIndexes()
        {
            var indexExists = false;
            var options = new ListIndexesOptions();
            var indexes = _database.GetCollection<CheckSheetTransaction>("check_sheet_transactions").Indexes.List(options);

            foreach (var index in indexes.ToEnumerable())
            {
                var indexDocument = index["key"].AsBsonDocument;
                if (indexDocument.Contains("CheckSheetDay"))
                {
                    indexExists = true;
                    break;
                }
            }

            if (!indexExists)
            {
                var keys = Builders<CheckSheetTransaction>.IndexKeys.Ascending(x => x.CheckSheetDay);
                var indexOptions = new CreateIndexOptions { Background = true }; 
                var model = new CreateIndexModel<CheckSheetTransaction>(keys, indexOptions);
                _database.GetCollection<CheckSheetTransaction>("check_sheet_transactions").Indexes.CreateOne(model);
            }

            CreateArchiveIndexes();
        }
        public void CreateArchiveIndexes()
        {
            var indexExists = false;
            var options = new ListIndexesOptions();
            var indexes = _database.GetCollection<CheckSheetTransactionArchive>("check_sheet_transactions_archive").Indexes.List(options);

            foreach (var index in indexes.ToEnumerable())
            {
                var indexDocument = index["key"].AsBsonDocument;
                if (indexDocument.Contains("CheckSheetDay"))
                {
                    indexExists = true;
                    break;
                }
            }

            if (!indexExists)
            {
                var keys = Builders<CheckSheetTransactionArchive>.IndexKeys.Ascending(x => x.CheckSheetDay);
                var indexOptions = new CreateIndexOptions { Background = true };
                var model = new CreateIndexModel<CheckSheetTransactionArchive>(keys, indexOptions);
                _database.GetCollection<CheckSheetTransactionArchive>("check_sheet_transactions_archive").Indexes.CreateOne(model);
            }
        }
        public async Task CreateAndPopulateTables()
        {
            var configCollectionExists = await CollectionExistsAsync("configuration");
            if (!configCollectionExists)
            {
                await _configCollection.Database.CreateCollectionAsync("configuration");
                await PopulateConfiguration();
            }

            var settingsCollectionExists = await CollectionExistsAsync("settings");
            if (!settingsCollectionExists)
            {
                await _settingsCollection.Database.CreateCollectionAsync("settings");
                await PopulateSettings();
            }
        }
        private async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collectionsCursor = await _configCollection.Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            var collectionExists = await collectionsCursor.AnyAsync();
            return collectionExists;
        }
        private List<CheckPoint> GetRandomCheckPoints()
        {
            var random = new Random();
            var checkPointCount = random.Next(5, 11);

            var checkPoints = new List<CheckPoint>();
            var nameList = new List<string>();
            nameList.Add("Main DB panel MCB's switch");
            nameList.Add("Function Tester Panel MCB's switches");
            nameList.Add("Function Tester Panel lights");
            nameList.Add("Push Button working");
            nameList.Add("Emergency Stop Working");
            nameList.Add("Bar code scanner");
            nameList.Add("Jig & wires for function testing");
            nameList.Add("Connector Wires");
            nameList.Add("Limit Switch");
            nameList.Add("Panel Fan Working");
            nameList.Add("Push to trip");

            var standardList = new List<string>();

            standardList.Add("No damage/On Condition");
            standardList.Add("Lights ON / No Damage");
            standardList.Add("ON/OFF & No Damage");
            standardList.Add("No damage / Working Condition/Red Light on");
            standardList.Add("No crimping loose/Insulation/Damage");
            standardList.Add("No Loose connections");
            standardList.Add("Stop Machine");
            standardList.Add("Working Properly");
            standardList.Add("Working Properly (MCB trip)");

            var methodlist = new List<string>();
            methodlist.Add("Visual / By hand");
            methodlist.Add("By hand / Visual");
            methodlist.Add("By Press");
            methodlist.Add("Visual");

            var conditionList = new List<string>();
            conditionList.Add("Before start work");

            for (int i = 1; i < checkPointCount; i++)
            {
                var randomNumber = random.Next(1, 31);
                string[] frequencyTypes = { "Daily", "Weekly", "Monthly", "Yearly" };
                var feqRandomNumber = random.Next(0, frequencyTypes.Length);
                string selectedFrequency = frequencyTypes[feqRandomNumber];
                checkPoints.Add(
                        new CheckPoint
                        {
                            Name = nameList[random.Next(0, nameList.Count)],
                            Standard = standardList[random.Next(0, standardList.Count)],
                            Method = methodlist[random.Next(0, methodlist.Count)],
                            Condition = conditionList[random.Next(0, conditionList.Count)],
                            SeqOrder = i,
                            CompleteInSeconds = random.Next(1, 10),
                            FileName = $"{randomNumber}.jpg",
                            UniqueFileName = $"65b33d45d26daf4ae1d1f3{randomNumber.ToString().PadLeft(2, '0')}.jpg",
                            FrequencyType = selectedFrequency,
                            WeekDays = selectedFrequency == "Weekly" ? new List<int> { random.Next(1, 3), random.Next(4, 6) } : new List<int>(),
                            MonthDays = selectedFrequency == "Monthly" ? new List<int> { random.Next(1, 5), random.Next(6, 10), random.Next(11, 15), random.Next(16, 20), random.Next(21, 25), random.Next(26, 30) } : new List<int>(),
                            YearlyMonths = selectedFrequency == "Yearly" ? new List<int> { random.Next(1, 3), random.Next(4, 6), random.Next(7, 9), random.Next(10, 12) } : new List<int>(),
                            YearlyMonthDays = selectedFrequency == "Yearly" ? new List<int> { random.Next(1, 5), random.Next(6, 10), random.Next(11, 20), random.Next(21, 31) } : new List<int>(),
                        }
                    );
            }
            return checkPoints;
        }
        private async Task PopulateConfiguration()
        {
            if (_env == "local" || _env == "development")
            {
                var plantId = ObjectId.GenerateNewId().ToString();

                var line1Id = ObjectId.GenerateNewId().ToString();
                var line2Id = ObjectId.GenerateNewId().ToString();
                var line3Id = ObjectId.GenerateNewId().ToString();
                var line4Id = ObjectId.GenerateNewId().ToString();
                var line5Id = ObjectId.GenerateNewId().ToString();
                var line6Id = ObjectId.GenerateNewId().ToString();

                var department1Id = ObjectId.GenerateNewId().ToString();
                var department2Id = ObjectId.GenerateNewId().ToString();
                var department3Id = ObjectId.GenerateNewId().ToString();
                var department4Id = ObjectId.GenerateNewId().ToString();
                var department5Id = ObjectId.GenerateNewId().ToString();

                var zone1Id = ObjectId.GenerateNewId().ToString();
                var zone2Id = ObjectId.GenerateNewId().ToString();
                var zone3Id = ObjectId.GenerateNewId().ToString();
                var zone4Id = ObjectId.GenerateNewId().ToString();
                var zone5Id = ObjectId.GenerateNewId().ToString();

                var equipment1Id = ObjectId.GenerateNewId().ToString();
                var equipment2Id = ObjectId.GenerateNewId().ToString();
                var equipment3Id = ObjectId.GenerateNewId().ToString();
                var equipment4Id = ObjectId.GenerateNewId().ToString();
                var equipment5Id = ObjectId.GenerateNewId().ToString();
                var equipment6Id = ObjectId.GenerateNewId().ToString();
                var equipment7Id = ObjectId.GenerateNewId().ToString();
                var equipment8Id = ObjectId.GenerateNewId().ToString();
                var equipment9Id = ObjectId.GenerateNewId().ToString();
                var equipment10Id = ObjectId.GenerateNewId().ToString();

                var station1Id = ObjectId.GenerateNewId().ToString();
                var station2Id = ObjectId.GenerateNewId().ToString();
                var station3Id = ObjectId.GenerateNewId().ToString();
                var station4Id = ObjectId.GenerateNewId().ToString();
                var station5Id = ObjectId.GenerateNewId().ToString();
                var station6Id = ObjectId.GenerateNewId().ToString();
                var station7Id = ObjectId.GenerateNewId().ToString();
                var station8Id = ObjectId.GenerateNewId().ToString();
                var station9Id = ObjectId.GenerateNewId().ToString();
                var station10Id = ObjectId.GenerateNewId().ToString();

                var location1Id = ObjectId.GenerateNewId().ToString();

                var sublocation1Id = ObjectId.GenerateNewId().ToString();

                var configs = new List<Configuration>() {
                new Configuration {
                    Id = plantId,
                    Name = "Plant",
                    Code ="RAJ",
                    Description = "Rajasthan"
                },
                new Configuration {
                    Name = "Plant",
                    Code ="MUM",
                    Description = "Mumbai"
                },
                new Configuration {
                    Name = "Plant",
                    Code ="KAR",
                    Description = "Karnataka"
                },
                new Configuration {
                    Name = "Plant",
                    Code ="HYD",
                    Description = "Hyderabad"
                },
                new Configuration {
                    Id  = line1Id,
                    Name = "Line",
                    Code = "R1",
                    Description = "R1",
                    CanDelete= false
                },
                new Configuration {
                    Id  = line2Id,
                    Name = "Line",
                    Code = "R2",
                    Description = "R2",
                    CanDelete= false
                },
                new Configuration {
                    Id  = line3Id,
                    Name = "Line",
                    Code = "R3",
                    Description = "R3",
                    CanDelete= false
                },
                new Configuration {
                    Id  = line4Id,
                    Name = "Line",
                    Code = "R4",
                    Description = "R4",
                    CanDelete= false
                },
                new Configuration {
                    Id  = line5Id,
                    Name = "Line",
                    Code = "R5",
                    Description = "R5",
                    CanDelete= false
                },
                new Configuration {
                    Id  = line6Id,
                    Name = "Line",
                    Code = "R6",
                    Description = "R6",
                    CanDelete= false
                },
                new Configuration {
                    Id  = department1Id,
                    Name = "Department",
                    Code = "PROD",
                    Description = "Production",
                    CanDelete= false
                },
                new Configuration {
                    Id  = department2Id,
                    Name = "Department",
                    Code = "MAIN",
                    Description = "Maintenance",
                    CanDelete= false
                },
                new Configuration {
                    Id  = department3Id,
                    Name = "Department",
                    Code = "QUA",
                    Description = "Quality",
                    CanDelete= false
                },
                new Configuration {
                    Id  = department4Id,
                    Name = "Department",
                    Code = "FIN",
                    Description = "Finance",
                    CanDelete= false
                },
                   new Configuration {
                    Id  = department5Id,
                    Name = "Department",
                    Code = "SC",
                    Description = "Supply Chain",
                    CanDelete= false
                },
                new Configuration {
                    Id  = zone1Id,
                    Name = "Zone",
                    Code = "1",
                    Description = "1"
                },
                new Configuration {
                    Id  = zone2Id,
                    Name = "Zone",
                    Code = "2",
                    Description = "2"
                },
                new Configuration {
                    Id  = zone3Id,
                    Name = "Zone",
                    Code = "3",
                    Description = "3"
                },
                new Configuration {
                    Id  = zone4Id,
                    Name = "Zone",
                    Code = "4",
                    Description = "4"
                },
                new Configuration {
                    Id  = zone5Id,
                    Name = "Zone",
                    Code = "5",
                    Description = "5"
                },
                new Configuration {
                    Id  = equipment1Id,
                    Name = "Equipment",
                    Code = "40001583",
                    Description = "Function Tester Panel",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = equipment2Id,
                    Name = "Equipment",
                    Code = "40001584",
                    Description = "Compressor Unit",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = equipment3Id,
                    Name = "Equipment",
                    Code = "40001585",
                    Description = "Heat Exchanger",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = equipment4Id,
                    Name = "Equipment",
                    Code = "40001586",
                    Description = "Control Panel",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = equipment5Id,
                    Name = "Equipment",
                    Code = "40001587",
                    Description = "Smart Theromstat",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = equipment6Id,
                    Name = "Equipment",
                    Code = "40001588",
                    Description = "Air Filteration System",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = equipment7Id,
                    Name = "Equipment",
                    Code = "40001589",
                    Description = "Evaporator Fan",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = equipment8Id,
                    Name = "Equipment",
                    Code = "40001590",
                    Description = "Condenser Coil",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = equipment9Id,
                    Name = "Equipment",
                    Code = "40001591",
                    Description = "Air Filter Assembly",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = equipment10Id,
                    Name = "Equipment",
                    Code = "40001592",
                    Description = "Expansion Valve",
                    CanDelete= true,
                },
                new Configuration {
                    Id  = station1Id,
                    Name = "Station",
                    Code = "STA01",
                    Description = "Station1",
                    CanDelete= true
                 },
                new Configuration {
                    Id  = station2Id,
                    Name = "Station",
                    Code = "STA02",
                    Description = "Station2",
                    CanDelete= true
                 },
                new Configuration {
                    Id  = station3Id,
                    Name = "Station",
                    Code = "STA03",
                    Description = "Station3",
                    CanDelete= true
                 },
                new Configuration {
                    Id  = station4Id,
                    Name = "Station",
                    Code = "STA04",
                    Description = "Station4",
                    CanDelete= true
                 },
                new Configuration {
                    Id  = station5Id,
                    Name = "Station",
                    Code = "STA05",
                    Description = "Station5",
                    CanDelete= true
                 },
                new Configuration {
                    Id  = station6Id,
                    Name = "Station",
                    Code = "STA06",
                    Description = "Station6",
                    CanDelete= true
                 },
                new Configuration {
                    Id  = station7Id,
                    Name = "Station",
                    Code = "STA07",
                    Description = "Station7",
                    CanDelete= true
                 },
                new Configuration {
                    Id  = station8Id,
                    Name = "Station",
                    Code = "STA08",
                    Description = "Station8",
                    CanDelete= true
                 },
                new Configuration {
                    Id  = station9Id,
                    Name = "Station",
                    Code = "STA09",
                    Description = "Station9",
                    CanDelete= true
                 },
                new Configuration {
                    Id  = station10Id,
                    Name = "Station",
                    Code = "STA10",
                    Description = "Station10",
                    CanDelete= true
                 },
                 new Configuration {
                    Id  = location1Id  ,
                    Name = "Location",
                    Code = "FACTORY-II",
                    Description = "FACTORY-II",
                    CanDelete= true
                 },
                 new Configuration {
                    Id  = sublocation1Id  ,
                    Name = "SubLocation",
                    Code = "EL Box Assembly",
                    Description = "EL Box Assembly",
                    CanDelete= true
                 }
                };
                await _configCollection.InsertManyAsync(configs);

                var users = new List<User>
                {
                    new User {
                        CanDelete = true,
                        DepartmentId = department1Id,
                        LoginId = "operator1",
                        Role = "Operator",
                        Email = "varmamkm@gmail.com",
                        FullName = "operator one",
                        PhoneNumber = "12345",
                        Password = "operator1",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id },
                    },
                    new User {
                        CanDelete = true,
                        DepartmentId = department2Id,
                        LoginId = "operator2",
                        Role = "Operator",
                        Email = "varmamkm@gmail.com",
                        FullName = "operator two",
                        PhoneNumber = "12345",
                        Password = "operator2",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id, line3Id },
                    },
                    new User {
                        CanDelete = true,
                        DepartmentId = department2Id,
                        LoginId = "validator1",
                        Role = "Validator",
                        Email = "varmamkm@gmail.com",
                        FullName = "validator one",
                        PhoneNumber = "12345",
                        Password = "validator1",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id, line3Id },
                    },
                    new User {
                        CanDelete = true,
                        DepartmentId = department1Id,
                        LoginId = "creator1",
                        Role = "Creator",
                        Email = "varmamkm@gmail.com",
                        FullName = "creator one",
                        PhoneNumber = "12345",
                        Password = "creator1",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id, line3Id },
                    },
                    new User {
                        CanDelete = true,
                        DepartmentId = department1Id,
                        LoginId = "reviewer1",
                        Role = "Reviewer",
                        Email = "varmamkm@gmail.com",
                        FullName = "reviewer one",
                        PhoneNumber = "12345",
                        Password = "reviewer1",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id, line3Id },
                    },
                    new User {
                        CanDelete = true,
                        DepartmentId = department2Id,
                        LoginId = "reviewer2",
                        Role = "Reviewer",
                        Email = "varmamkm@gmail.com",
                        FullName = "reviewer two",
                        PhoneNumber = "12345",
                        Password = "reviewer2",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id, line3Id },
                    },
                    new User {
                        CanDelete = true,
                        DepartmentId = department3Id,
                        LoginId = "reviewer3",
                        Role = "Reviewer",
                        Email = "varmamkm@gmail.com",
                        FullName = "reviewer three",
                        PhoneNumber = "12345",
                        Password = "reviewer3",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id, line3Id },
                    },
                    new User {
                        CanDelete = true,
                        DepartmentId = department1Id,
                        LoginId = "approver1",
                        Role = "Approver",
                        Email = "varmamkm@gmail.com",
                        FullName = "approver one",
                        PhoneNumber = "12345",
                        Password = "approver1",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id, line3Id },
                    },
                    new User {
                        CanDelete = true,
                        DepartmentId = department2Id,
                        LoginId = "approver2",
                        Role = "Approver",
                        Email = "varmamkm@gmail.com",
                        FullName = "approver two",
                        PhoneNumber = "12345",
                        Password = "approver2",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id, line3Id },
                    },
                    new User {
                        CanDelete = false,
                        DepartmentId = department2Id,
                        LoginId = "superadmin",
                        Role = "SuperAdmin",
                        Email = "varmamkm@gmail.com",
                        FullName = "superadmin",
                        PhoneNumber = "12345",
                        Password = "superadmin",
                        PlantId = plantId,
                        LineIds = new List<string>{ line1Id, line2Id, line3Id },
                    }
                };
                await _userCollection.InsertManyAsync(users);

                var checkSheets = new List<CheckSheet>() {
                new CheckSheet()
                {
                    UniqueId = "PM VIII-EL010(F2)",
                    Name = "Running Test Equipment",
                    LineId = line1Id,
                    DepartmentId = department1Id,
                    MaintenaceClassId = zone1Id,
                    EquipmentId = equipment1Id,
                    StationId = station1Id,
                    LocationId = location1Id,
                    SubLocationId = sublocation1Id,
                    CheckPoints = GetRandomCheckPoints()
                },
                new CheckSheet()
                {
                    UniqueId = "CU VIII-EL010",
                    Name = "Compressor Unit",
                    LineId = line4Id,
                    DepartmentId = department2Id,
                    MaintenaceClassId = zone2Id,
                    EquipmentId = equipment2Id,
                    StationId = station4Id,
                    LocationId = location1Id,
                    SubLocationId = sublocation1Id,
                    CheckPoints = GetRandomCheckPoints()
                },

                new CheckSheet()
                {
                    UniqueId = "HE VX-HX012",
                    Name = "Heat Exchanger",
                    LineId = line3Id,
                    DepartmentId = department3Id,
                    MaintenaceClassId = zone5Id,
                    EquipmentId = equipment3Id,
                    StationId = station7Id,
                    LocationId = location1Id,
                    SubLocationId = sublocation1Id,
                    CheckPoints = GetRandomCheckPoints()
                },
                new CheckSheet()
                {
                    UniqueId = "CPM-001",
                    Name = "Control Panel Maintenance Check",
                    LineId = line2Id,
                    DepartmentId = department1Id,
                    MaintenaceClassId = zone5Id,
                    EquipmentId = equipment4Id,
                    StationId = station10Id,
                    LocationId = location1Id,
                    SubLocationId = sublocation1Id,
                    CheckPoints = GetRandomCheckPoints()
                },
                new CheckSheet()
                {
                     UniqueId = "ST SX-RF011",
                     Name = "Smart Thermostat",
                     LineId = line6Id,
                     DepartmentId = department2Id,
                     MaintenaceClassId = zone3Id,
                     EquipmentId = equipment5Id,
                     StationId = station5Id,
                     LocationId = location1Id,
                     SubLocationId = sublocation1Id,
                     CheckPoints = GetRandomCheckPoints()
                },

                new CheckSheet()
                {
                    UniqueId = "AFS-PM-I",
                    Name = "Air Filtration System Inspection",
                    LineId = line5Id,
                    DepartmentId = department2Id,
                    MaintenaceClassId = zone3Id,
                    EquipmentId = equipment6Id,
                    StationId = station7Id,
                    LocationId = location1Id,
                    SubLocationId = sublocation1Id,
                    CheckPoints = GetRandomCheckPoints()
                },

                new CheckSheet()
                {
                    UniqueId = "EF-EE-I01",
                    Name = "Evaporator Fan",
                    LineId = line1Id,
                    DepartmentId = department1Id,
                    MaintenaceClassId = zone5Id,
                    EquipmentId = equipment7Id,
                    StationId = station4Id,
                    LocationId = location1Id,
                    SubLocationId = sublocation1Id,
                    CheckPoints = GetRandomCheckPoints()
                },
                new CheckSheet()
                {
                    UniqueId = "CC0012-PM-II",
                    Name = "Condenser Coil Inspection",
                    LineId = line2Id,
                    DepartmentId = department3Id,
                    MaintenaceClassId = zone5Id,
                    EquipmentId = equipment8Id,
                    StationId = station3Id,
                    LocationId = location1Id,
                    SubLocationId = sublocation1Id,
                    CheckPoints = GetRandomCheckPoints()
                },
                new CheckSheet()
                 {
                    UniqueId = "AF AA-RF001",
                    Name = "Air Filter Assembly",
                    LineId = line4Id,
                    DepartmentId = department2Id,
                    MaintenaceClassId = zone2Id,
                    EquipmentId = equipment9Id,
                    StationId = station10Id,
                    LocationId = location1Id,
                    SubLocationId = sublocation1Id,
                    CheckPoints = GetRandomCheckPoints()
                },
                new CheckSheet()
                {
                    UniqueId = "PM VIII-EL011(F2)",
                    Name = "Expansion Valve Inspection",
                    LineId = line4Id,
                    DepartmentId = department3Id,
                    MaintenaceClassId = zone4Id,
                    EquipmentId = equipment10Id,
                    StationId = station7Id,
                    LocationId = location1Id,
                    SubLocationId = sublocation1Id,
                    CheckPoints = GetRandomCheckPoints()
                }

            };
                await _checkSheetCollection.InsertManyAsync(checkSheets);

                var checkSheetVersions = _mapper.Map<List<CheckSheetVersion>>(checkSheets);
                foreach (var checkSheetVersion in checkSheetVersions)
                {
                    checkSheetVersion.Status = "Approved";
                    checkSheetVersion.Version = 1;
                    checkSheetVersion.IsApproved = true;
                    checkSheetVersion.ActivateOn = DateTime.UtcNow.Date + TimeSpan.Zero;
                    checkSheetVersion.IsReviewed = true;
                    checkSheetVersion.ChangeDetails = "Initial Version";
                    checkSheetVersion.Reviewers = new List<Reviewer> {
                        new Reviewer {Department = "Production", IsReviewed = true, ReviewedOn = DateTime.UtcNow, ReviewerName = "Reviewer One" , Email = "varmamkm@gmail.com"},
                        new Reviewer {Department = "Maintenance", IsReviewed = true, ReviewedOn = DateTime.UtcNow, ReviewerName = "Reviewer Two"  , Email = "varmamkm@gmail.com"},
                        new Reviewer {Department = "Quality", IsReviewed = true, ReviewedOn = DateTime.UtcNow, ReviewerName = "Reviewer Three"  , Email = "varmamkm@gmail.com"},
                    };
                    checkSheetVersion.Approvers = new List<Approver> {
                        new Approver {Department = "Production", IsApproved = true, ApprovedOn = DateTime.UtcNow, ApproverName = "Approver One"  , Email = "varmamkm@gmail.com"},
                        new Approver {Department = "Maintenance", IsApproved = true, ApprovedOn = DateTime.UtcNow, ApproverName = "Approver Two"  , Email = "varmamkm@gmail.com"}
                    };
                }
                await _checkSheetVersionCollection.InsertManyAsync(checkSheetVersions);
            }
            else if (_env == "uat")
            {
                var users = new List<User>
                {
                    new User {
                        CanDelete = false,
                        LoginId = "superadmin",
                        Role = "SuperAdmin",
                        Email = "varmamkm@gmail.com",
                        FullName = "superadmin",
                        PhoneNumber = "12345",
                        Password = "superadmin",
                    }
                };
                await _userCollection.InsertManyAsync(users);
            }
            else if (_env == "production")
            {
                var users = new List<User>
                {
                    new User {
                        CanDelete = false,
                        LoginId = "superadmin",
                        Role = "SuperAdmin",
                        Email = "varmamkm@gmail.com",
                        FullName = "superadmin",
                        PhoneNumber = "12345",
                        Password = "superadmin",
                    }
                };
                await _userCollection.InsertManyAsync(users);
            }
        }
        private async Task PopulateSettings()
        {
            var settingsId = "662a1be9e314ceb036814155";

            var masterSettings = new MasterSettings
            {
                Id = settingsId,
                CreatedBy = "SYS",
                CreatedOn = DateTime.Now,
                IsActive = true,
                Locktime = 5,
                SenderEmailAddress = "no-reply@pocapi.in",
                SMTPEnableSSL = true,
                SMTPHost = "smtpout.secureserver.net",
                SMTPPort = 587,
                SMTPPassword = "Welcome@123",
                SMTPUserId = "no-reply@pocapi.in",
                Shifts = new List<Shift>
                {
                    new Shift
                    {
                        Name = "A",
                        StartTime = "06:15",
                        EndTime = "14:40"
                    },
                    new Shift
                    {
                        Name = "B",
                        StartTime = "14:40",
                        EndTime = "22:50"
                    },
                    new Shift
                    {
                        Name = "C",
                        StartTime = "22:50",
                        EndTime = "06:00"
                    },
                    new Shift
                    {
                        Name = "G",
                        StartTime = "08:40",
                        EndTime = "17:40"
                    }
                }
            };
            await _settingsCollection.InsertOneAsync(masterSettings);
        }
    }
}
