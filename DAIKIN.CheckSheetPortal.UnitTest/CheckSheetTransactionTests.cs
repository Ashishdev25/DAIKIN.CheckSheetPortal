using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using Moq;
using Moq.Language.Flow;
using System;
using AutoMapper;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using DAIKIN.CheckSheetPortal.Services;
using Microsoft.Extensions.Logging;
using DAIKIN.CheckSheetPortal.DTO;
using System.Runtime.InteropServices;
using DAIKIN.CheckSheetPortal.Infrastructure.DataAccess;
using DAIKIN.CheckSheetPortal.DataAccess;

namespace DAIKIN.CheckSheetPortal.UnitTest
{
    [TestClass]
    public class CheckSheetTransactionTests
    {
        private Mock<ICheckSheetTransactionRepository> _repositoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ICacheService> _cacheMock;
        private Mock<ICheckSheetImageService> _imageService;
        private Mock<ILogger<CheckSheetTransactionService>> _logger;



        [TestInitialize]
        public void Setup()
        {
            //_repositoryMock = new CheckSheetTransactionRepository();

            _mapperMock = new Mock<IMapper>();
            _cacheMock = new Mock<ICacheService>();
            _imageService = new Mock<ICheckSheetImageService>();
            _logger = new Mock<ILogger<CheckSheetTransactionService>>();

        }
        [TestMethod]
        public async Task GetAllAsync_ShouldReturn_NoRecordsFound()
        {
            var CatchedItems = new List<CheckSheetTransaction>
            {

            };
            _cacheMock.Setup(c => c.Get<List<CheckSheetTransaction>>("CheckSheetTransaction")).Returns(CatchedItems);

            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
            //var result = await service.GetAllAsync();
            //Assert.IsNotNull(result);
            //Assert.IsFalse(result.IsSuccess);
            //Assert.AreEqual("No Records found", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task GetAllAsync_ShouldReturnItemsFromRepository_WhenCacheIsEmpty()
        {
            _cacheMock.Setup(c => c.Get<IEnumerable<CheckSheetTransaction>>("CheckSheetTransaction")).Returns((IEnumerable<CheckSheetTransaction>)null);

            var CatchedItems = new List<CheckSheetTransaction>
            {
                 new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3a",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3c",
                    Name="Air Filter assembly",
                    Line="R6",
                    UniqueId="A1102",
                    Equipment="Air Filter",
                    EquipmentCode="EA0001",
                    Station = "STA01",
                    Department="production",
                    Location="FACTORY-II",
                    MaintenaceClass="1",
                    SubLocation="EL Box Assembly",
                    CheckSheetDay=DateTime.Parse("04-02-2024"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Filter Light indication",
                            Standard="Green Light",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Air Filter Fan",
                            Standard="Running Condition",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        }
                    }
                }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(CatchedItems);

            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
            //var result = await service.GetAllAsync();
            //// Assert
            //Assert.IsNotNull(result);
            //Assert.IsTrue(result.IsSuccess);
            //var items = ((OperationResponse<IEnumerable<CheckSheetTransaction>>)result).Payload;
        }
        [TestMethod]
        public async Task GetAllAsync_ShouldReturnItemsFromRepository_WhenCacheIsNotEmpty()
        {

            var CatchedItems = new List<CheckSheetTransaction>
            {
                 new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3a",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3c",
                    Name="Air Filter assembly",
                    Line="R6",
                    UniqueId="A1102",
                    Equipment="Air Filter",
                    EquipmentCode="EA0001",
                    Station = "STA01",
                    Department="production",
                    Location="FACTORY-II",
                    MaintenaceClass="1",
                    SubLocation="EL Box Assembly",
                    CheckSheetDay=DateTime.Parse("04-02-2024"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Filter Light indication",
                            Standard="Green Light",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Air Filter Fan",
                            Standard="Running Condition",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        }
                    }
                },
                  new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3f",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3g",
                    Name="Smart Theromstat",
                    Line="R2",
                    UniqueId="ST SX-RF011",
                    Equipment="Smart Theromstat",
                    EquipmentCode="40001587",
                    Department="Production",
                    Station = "Station5",
                    Location="FACTORY-II",
                    SubLocation="EL Box Assem",
                    MaintenaceClass="1",
                    CheckSheetDay=DateTime.Parse("04-02-2024"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Emergency Stop Working",
                            Standard="No damage / Working Condition/Red Light on",
                            Method="By Press",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord= null,
                            Comments = null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Push Button working",
                            Standard="Stop Machine",
                            Method="Visual/Hand",
                            Condition="By Press",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord= null,
                            Comments = null,
                            IsActive=true
                        }
                    }
                }
            };
            _cacheMock.Setup(c => c.Get<IEnumerable<CheckSheetTransaction>>("CheckSheetTransaction")).Returns(CatchedItems);

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(CatchedItems);

            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
            //var result = await service.GetAllAsync();
            //var items = ((OperationResponse<IEnumerable<CheckSheetTransaction>>)result).Payload;
            //// Assert
            //Assert.IsNotNull(result);
            //Assert.IsTrue(result.IsSuccess);
            //CollectionAssert.AreEqual(CatchedItems, (System.Collections.ICollection)items);
        }
        [TestMethod]
        public async Task GetCheckSheetsAsync_Should_ReturnError_NoRecordsFound()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
                new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3a",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3c",
                    Name="Air Filter assembly",
                    Line="R6",
                    UniqueId="A1102",
                    Equipment="Air Filter",
                    EquipmentCode="EA0001",
                    Station = "STA01",
                    Department="production",
                    Location="FACTORY-II",
                    MaintenaceClass="1",
                    SubLocation="EL Box Assembly",
                    CheckSheetDay=DateTime.Parse("2024-02-03T00:00:00Z"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Filter Light indication",
                            Standard="Green Light",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Air Filter Fan",
                            Standard="Running Condition",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        }
                    }
                }

            };
            // Mock setup for repository
            _repositoryMock.Setup(repo => repo.GetByFilterAsync("CheckSheetDay", "2024-02-04T00:00:00Z")).ReturnsAsync(checksheetstransaction);
            _mapperMock.Setup(mapper => mapper.Map<CheckSheetTransactionDTO>(It.IsAny<CheckSheetTransaction>()))
          .Returns((CheckSheetTransaction source) => new CheckSheetTransactionDTO
          {
              Id = source.Id,
              Name = source.Name,
              Line = source.Line,
              UniqueId = source.UniqueId,
              Equipment = source.Equipment,
              EquipmentCode = source.EquipmentCode,
              Department = source.Department,
              Station = source.Station,
              Location = source.Location,
              SubLocation = source.SubLocation,
              MaintenaceClass = source.MaintenaceClass,
              CheckSheetDay = source.CheckSheetDay
          });

            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);

            //// Act
            //var result = await service.GetCheckSheetsAsync("", new UserDTO());
            //// Assert
            //Assert.IsNotNull(result);
            //Assert.IsFalse(result.IsSuccess);
            //Assert.AreEqual("No Records found", result.Messages.ToList()[0]);


        }
        [TestMethod]
        public async Task GetCheckSheetsAsync_Should_ReturnCheckSheet_for_the_currentday()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
                new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3a",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3c",
                    Name="Air Filter assembly",
                    Line="R6",
                    UniqueId="A1102",
                    Equipment="Air Filter",
                    EquipmentCode="EA0001",
                    Station = "STA01",
                    Department="production",
                    Location="FACTORY-II",
                    MaintenaceClass="1",
                    SubLocation="EL Box Assembly",
                    CheckSheetDay=DateTime.Parse("2024-02-05T00:00:00Z"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Filter Light indication",
                            Standard="Green Light",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Air Filter Fan",
                            Standard="Running Condition",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        }
                    }
                },
                 new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3f",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3g",
                    Name="Smart Theromstat",
                    Line="R2",
                    UniqueId="ST SX-RF011",
                    Equipment="Smart Theromstat",
                    EquipmentCode="40001587",
                    Department="Production",
                    Station = "Station5",
                    Location="FACTORY-II",
                    SubLocation="EL Box Assem",
                    MaintenaceClass="1",
                    CheckSheetDay=DateTime.Parse("2024-02-05T00:00:00Z"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Emergency Stop Working",
                            Standard="No damage / Working Condition/Red Light on",
                            Method="By Press",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord= null,
                            Comments = null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Push Button working",
                            Standard="Stop Machine",
                            Method="Visual/Hand",
                            Condition="By Press",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord= null,
                            Comments = null,
                            IsActive=true
                        }
                    }
                }

            };
            _repositoryMock.Setup(repo => repo.GetByFilterAsync("CheckSheetDay", DateTime.UtcNow.Date + TimeSpan.Zero)).ReturnsAsync(checksheetstransaction);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<CheckSheetTransactionDTO>>(It.IsAny<IEnumerable<CheckSheetTransaction>>()))
          .Returns(checksheetstransaction.Select(item => new CheckSheetTransactionDTO
          {
              Id = item.Id,
              Name = item.Name,
              Line = item.Line,
              UniqueId = item.UniqueId,
              Equipment = item.Equipment,
              EquipmentCode = item.EquipmentCode,
              Department = item.Department,
              Station = item.Station,
              Location = item.Location,
              SubLocation = item.SubLocation,
              MaintenaceClass = item.MaintenaceClass,
              CheckSheetDay = item.CheckSheetDay,
          }));
            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
            //// Act
            //var result = await service.GetCheckSheetsAsync("");

            //// Assert
            //Assert.IsNotNull(result);
            //Assert.IsTrue(result.IsSuccess);

            //var operationResponse = (OperationResponse<IEnumerable<CheckSheetTransactionDTO>>)result;
            //Assert.IsNotNull(operationResponse.Payload, "The payload should not be null");

            //var payload = operationResponse.Payload.ToList(); // Convert to List for comparison
            //Assert.AreEqual(checksheetstransaction.Count, payload.Count, "Payload count should match");

        }
        [TestMethod]
        public async Task GetCheckSheetByIdAsync_Should_ReturnErrorNoRecordsNotFound__for_InvalidId()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
                new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3a",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3c",
                    Name="Air Filter assembly",
                    Line="R6",
                    UniqueId="A1102",
                    Equipment="Air Filter",
                    EquipmentCode="EA0001",
                    Station = "STA01",
                    Department="production",
                    Location="FACTORY-II",
                    MaintenaceClass="1",
                    SubLocation="EL Box Assembly",
                    CheckSheetDay=DateTime.Parse("04-02-2024"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Filter Light indication",
                            Standard="Green Light",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Air Filter Fan",
                            Standard="Running Condition",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        }
                    }
                },
                 new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3f",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3g",
                    Name="Smart Theromstat",
                    Line="R2",
                    UniqueId="ST SX-RF011",
                    Equipment="Smart Theromstat",
                    EquipmentCode="40001587",
                    Department="Production",
                    Station = "Station5",
                    Location="FACTORY-II",
                    SubLocation="EL Box Assem",
                    MaintenaceClass="1",
                    CheckSheetDay=DateTime.Parse("04-02-2024"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Emergency Stop Working",
                            Standard="No damage / Working Condition/Red Light on",
                            Method="By Press",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord= null,
                            Comments = null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Push Button working",
                            Standard="Stop Machine",
                            Method="Visual/Hand",
                            Condition="By Press",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord= null,
                            Comments = null,
                            IsActive=true
                        }
                    }
                }
            };
            _mapperMock.Setup(mapper => mapper.Map<CheckSheetTransactionFullDTO>(It.IsAny<CheckSheetTransaction>()))
              .Returns((CheckSheetTransactionFullDTO source) => new CheckSheetTransactionFullDTO
              {
                  Id = source.Id,
                  CheckSheetId = source.CheckSheetId,
                  Name = source.Name,
                  Line = source.Line,
                  UniqueId = source.UniqueId,
                  Equipment = source.Equipment,
                  EquipmentCode = source.EquipmentCode,
                  Department = source.Department,
                  Station = source.Station,
                  Location = source.Location,
                  SubLocation = source.SubLocation,
                  MaintenaceClass = source.MaintenaceClass,
                  CheckSheetDay = source.CheckSheetDay,
                  IsActive = source.IsActive,
                  CheckPointTransactions = source.CheckPointTransactions?.Select(cp => new CheckPointTransactionDTO
                  {
                      Name = cp.Name,
                      Standard = cp.Standard,
                      Method = cp.Method,
                      Condition = cp.Condition,
                      SeqOrder = cp.SeqOrder,
                      FrequencyType = cp.FrequencyType,
                      CheckRecord = cp.CheckRecord,
                      Comments = cp.Comments
                  }).ToList()
              });

            // Mock setup for repository
            //_repositoryMock.Setup(repo => repo.GetByIdAsync("65bee0eb6a517a1cc3a81b3c"))
            //    .ReturnsAsync((string id) => checksheetstransaction.FirstOrDefault(c => c.Id == id));

            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);

            //// Act
            //var result = await service.GetCheckSheetByIdAsync("65bee0eb6a517a1cc3a81b3f");
            //Assert.IsNotNull(result);
            //Assert.IsFalse(result.IsSuccess);
            //Assert.AreEqual("No Records found", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task GetCheckSheetByIdAsync_Should_ReturnRecord_for_validId()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
                new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3a",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3c",
                    Name="Air Filter assembly",
                    Line="R6",
                    UniqueId="A1102",
                    Equipment="Air Filter",
                    EquipmentCode="EA0001",
                    Station = "STA01",
                    Department="production",
                    Location="FACTORY-II",
                    MaintenaceClass="1",
                    SubLocation="EL Box Assembly",
                    CheckSheetDay=DateTime.Parse("04-02-2024"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Filter Light indication",
                            Standard="Green Light",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Air Filter Fan",
                            Standard="Running Condition",
                            Method="Visual/Hand",
                            Condition="Before start work",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord=null,
                            Comments=null,
                            IsActive=true
                        }
                    }
                },
                 new CheckSheetTransaction
                {
                    Id ="65bee0eb6a517a1cc3a81b3f",
                    CheckSheetId="65bee0eb6a517a1cc3a81b3g",
                    Name="Smart Theromstat",
                    Line="R2",
                    UniqueId="ST SX-RF011",
                    Equipment="Smart Theromstat",
                    EquipmentCode="40001587",
                    Department="Production",
                    Station = "Station5",
                    Location="FACTORY-II",
                    SubLocation="EL Box Assem",
                    MaintenaceClass="1",
                    CheckSheetDay=DateTime.Parse("04-02-2024"),
                    IsActive=true,
                    CheckPointTransactions = new List<CheckPointTransaction>
                    {
                        new CheckPointTransaction
                        {
                            Name="Emergency Stop Working",
                            Standard="No damage / Working Condition/Red Light on",
                            Method="By Press",
                            Condition="Before start work",
                            SeqOrder=1,
                            FrequencyType="daily",
                            CheckRecord= null,
                            Comments = null,
                            IsActive=true
                        },
                        new CheckPointTransaction
                        {
                            Name="Push Button working",
                            Standard="Stop Machine",
                            Method="Visual/Hand",
                            Condition="By Press",
                            SeqOrder=2,
                            FrequencyType="daily",
                            CheckRecord= null,
                            Comments = null,
                            IsActive=true
                        }
                    }
                }
            };
            _mapperMock.Setup(mapper => mapper.Map<CheckSheetTransactionDTO>(It.IsAny<CheckSheetTransaction>()))
              .Returns((CheckSheetTransaction source) => new CheckSheetTransactionDTO
              {
                  Id = source.Id,
                  CheckSheetId = source.CheckSheetId,
                  Name = source.Name,
                  Line = source.Line,
                  UniqueId = source.UniqueId,
                  Equipment = source.Equipment,
                  EquipmentCode = source.EquipmentCode,
                  Department = source.Department,
                  Station = source.Station,
                  Location = source.Location,
                  SubLocation = source.SubLocation,
                  MaintenaceClass = source.MaintenaceClass,
                  CheckSheetDay = source.CheckSheetDay,
                  IsActive = source.IsActive
              });

            // Mock setup for repository
            _repositoryMock.Setup(repo => repo.GetByIdAsync("65bee0eb6a517a1cc3a81b3f"))
                .ReturnsAsync((string id) => checksheetstransaction.FirstOrDefault(c => c.Id == id));

            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);

            //// Act
            //var result = await service.GetCheckSheetByIdAsync("65bee0eb6a517a1cc3a81b3f");
            //// Assert
            //Assert.IsNotNull(result);
            //Assert.IsTrue(result.IsSuccess);
            //var payload = ((OperationResponse<CheckSheetTransactionDTO>)result).Payload;

            //Assert.IsNotNull(payload, "The payload should not be null");
            //Assert.AreEqual("65bee0eb6a517a1cc3a81b3f", payload.Id, "The IDs should match");


        }
        [TestMethod]
        public async Task UpdateCheckRecordAsync_ShouldReturnError_WhenCheckSheetIdIsNotFound()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
     {
         new CheckSheetTransaction
         {
             Id ="CS1",
             Name="Air Filter assembly",
             Line="R6",
             UniqueId="A1102",
             Equipment="Air Filter",
             EquipmentCode="EA0001",
             Station = "STA01",
             Location="FACTORY-II",
             SubLocation="EL Box Assembly",
             CheckSheetDay=DateTime.Parse("2024-02-02T00:00:00Z"),
             StartedBy="SYS",
             StartedOn=DateTime.Parse("2024-02-02T00:00:00Z"),
             Status="Not Started",
             ColorCode="Black",
             IsActive=true,
             IsLocked=false,
             CheckPoints = new List<CheckPoint>
             {
                 new CheckPointTransaction
                 {
                     Id="CP1",
                     Name="Filter Light indication",
                     Standard="Green Light",
                     Method="Visual/Hand",
                     SeqOrder=1,
                     FrequencyType="daily",
                     CheckRecord="OK",
                     Comments="",
                     IsForToday=true,
                 },
                 new CheckPointTransaction
                 {
                     Id="CP2",
                     Name="Air Filter Fan",
                     Standard="Running Condition",
                     Method="Visual/Hand",
                     SeqOrder=1,
                     FrequencyType="daily",
                     CheckRecord="OK",
                     Comments="",
                     IsForToday=true,
                 }
             }
         }
     };
           

            var EntryDTO = new CheckPointEntryDTO
            {
                CheckSheetId = "cs2",
                CheckPointId = "CP1",
                CheckRecord = "OK",
                Comments = null,
                UserAction = "Submitted"
            };
            //_repositoryMock.Setup(repo => repo.GetByIdAsync(EntryDTO.CheckSheetId)).ReturnsAsync((string id) => checksheetstransaction.FirstOrDefault(c => c.Id == id));

            //_cacheMock.Setup(c => c.Get<List<CheckSheetTransaction>>("CheckSheetTransaction")).Returns(checksheetstransaction);
            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
           
            //var result = await service.UpdateCheckRecordAsync(EntryDTO);
            //Assert.IsNotNull(result);
            //Assert.IsFalse(result.IsSuccess);
            //Assert.AreEqual($"Check sheet with id {EntryDTO.CheckSheetId} not found", result.Messages.ToList()[0]);
        }

        [TestMethod]
        public async Task UpdateCheckRecordAsync_ShouldReturnErrorCommentsIsEmpty_WhenCheckRecordIsNG()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
            new CheckSheetTransaction
            {
                Id ="CS1",
                Name="Air Filter assembly",
                Line="R6",
                UniqueId="A1102",
                Equipment="Air Filter",
                EquipmentCode="EA0001",
                Station = "STA01",
                Location="FACTORY-II",
                SubLocation="EL Box Assembly",
                CheckSheetDay=DateTime.Parse("2024-02-02T00:00:00Z"),
                StartedBy="SYS",
                StartedOn=DateTime.Parse("2024-02-02T00:00:00Z"),
                Status="Not Started",
                ColorCode="Black",
                IsActive=true,
                IsLocked=false,
                CheckPoints = new List<CheckPoint>
                {
                    new CheckPointTransaction
                    {
                        Id="CP1",
                        Name="Filter Light indication",
                        Standard="Green Light",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    },
                    new CheckPointTransaction
                    {
                        Id="CP2",
                        Name="Air Filter Fan",
                        Standard="Running Condition",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    }
                }
            }
          };
            var EntryDTO = new CheckPointEntryDTO
            {
                CheckSheetId = "CS1",
                CheckPointId = "CP1",
                CheckRecord = "NG",
                Comments = null,
                UserAction = "Submitted"
            };
            _repositoryMock.Setup(repo => repo.GetByIdAsync(EntryDTO.CheckSheetId)).ReturnsAsync((string id) => checksheetstransaction.FirstOrDefault(c => c.Id == id));

            //_cacheMock.Setup(c => c.Get<List<CheckSheetTransaction>>("CheckSheetTransaction")).Returns(checksheetstransaction);
            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
           
            //var result = await service.UpdateCheckRecordAsync(EntryDTO);
            //Assert.IsNotNull(result);
            //Assert.IsFalse(result.IsSuccess);
            //Assert.AreEqual("Comments cannot be empty for NG", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task UpdateCheckRecordAsync_ShouldReturnError_WhenCheckSheetStartedByisNotSYS()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
              new CheckSheetTransaction
              {
                 Id ="CS1",
                 Name="Air Filter assembly",
                 Line="R6",
                 UniqueId="A1102",
                 Equipment="Air Filter",
                 EquipmentCode="EA0001",
                 Station = "STA01",
                 Location="FACTORY-II",
                 SubLocation="EL Box Assembly",
                 CheckSheetDay=DateTime.Parse("2024-02-02T00:00:00Z"),
                 StartedBy="Admin",
                 StartedOn=DateTime.Parse("2024-02-02T00:00:00Z"),
                 Status="Not Started",
                 ColorCode="Black",
                 IsActive=true,
                 IsLocked=true,
                 CheckPoints = new List<CheckPoint>
                 {
                     new CheckPointTransaction
                     {
                         Id="CP1",
                         Name="Filter Light indication",
                         Standard="Green Light",
                         Method="Visual/Hand",
                         SeqOrder=1,
                         FrequencyType="daily",
                         CheckRecord="OK",
                         Comments="",
                         IsForToday=true,
                     },
                     new CheckPointTransaction
                     {
                         Id="CP2",
                         Name="Air Filter Fan",
                         Standard="Running Condition",
                         Method="Visual/Hand",
                         SeqOrder=1,
                         FrequencyType="daily",
                         CheckRecord="OK",
                         Comments="",
                         IsForToday=true,
                     }
                 }
              }
            };
            var EntryDTO = new CheckPointEntryDTO
            {
                CheckSheetId = "CS1",
                CheckPointId = "CP1",
                CheckRecord = "OK",
                Comments = null,
                UserAction = "Submitted"
            };
            //_repositoryMock.Setup(repo => repo.GetByIdAsync(EntryDTO.CheckSheetId)).ReturnsAsync((string id) => checksheetstransaction.FirstOrDefault(c => c.Id == id));
            //_cacheMock.Setup(c => c.Get<List<CheckSheetTransaction>>("CheckSheetTransaction")).Returns(checksheetstransaction);
            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);

            //var result = await service.UpdateCheckRecordAsync(EntryDTO);
            //Assert.IsNotNull(result);
            //Assert.IsFalse(result.IsSuccess);
            //Assert.AreEqual($"The selected checksheet is in in-progress by {checksheetstransaction[0].StartedBy}", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task BulkUpdateCheckRecordAsync_ShouldReturnError_WhenCheckRecordIsNG()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
            new CheckSheetTransaction
            {
                Id ="CS1",
                Name="Air Filter assembly",
                Line="R6",
                UniqueId="A1102",
                Equipment="Air Filter",
                EquipmentCode="EA0001",
                Station = "STA01",
                Location="FACTORY-II",
                SubLocation="EL Box Assembly",
                CheckSheetDay=DateTime.Parse("2024-02-02T00:00:00Z"),
                StartedBy="SYS",
                StartedOn=DateTime.Parse("2024-02-02T00:00:00Z"),
                Status="Not Started",
                ColorCode="Black",
                IsActive=true,
                IsLocked=false,
                CheckPoints = new List<CheckPoint>
                {
                    new CheckPointTransaction
                    {
                        Id="CP1",
                        Name="Filter Light indication",
                        Standard="Green Light",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    },
                    new CheckPointTransaction
                    {
                        Id="CP2",
                        Name="Air Filter Fan",
                        Standard="Running Condition",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    }
                }
            }
           };
            var checkPointBulkEntryDTO = new CheckPointEntryDTO
            {
                CheckSheetId = "CS1",
                CheckPointId = "CP1",
                CheckRecord = "NG",
                Comments = "Not Good",
                UserAction = "Submitted"
            };
            //_repositoryMock.Setup(repo => repo.GetByIdAsync(checkPointBulkEntryDTO.CheckSheetId)).ReturnsAsync((string id) => checksheetstransaction.FirstOrDefault(c => c.Id == id));

            //_cacheMock.Setup(c => c.Get<List<CheckSheetTransaction>>("CheckSheetTransaction")).Returns(checksheetstransaction);
            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
           
            //var result = await service.BulkUpdateCheckRecordAsync(checkPointBulkEntryDTO);
            //Assert.IsNotNull(result);
            //Assert.IsFalse(result.IsSuccess);
            //Assert.AreEqual("NG record is not allowed for Bulk update", result.Messages.ToList()[0]);
        }

        [TestMethod]
        public async Task BulkUpdateCheckRecordAsync_ShouldReturnError_WhenCheckSheetIdIsNotFound()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
            new CheckSheetTransaction
            {
                Id ="CS1",
                Name="Air Filter assembly",
                Line="R6",
                UniqueId="A1102",
                Equipment="Air Filter",
                EquipmentCode="EA0001",
                Station = "STA01",
                Location="FACTORY-II",
                SubLocation="EL Box Assembly",
                CheckSheetDay=DateTime.Parse("2024-02-02T00:00:00Z"),
                StartedBy="SYS",
                StartedOn=DateTime.Parse("2024-02-02T00:00:00Z"),
                Status="Not Started",
                ColorCode="Black",
                IsActive=true,
                IsLocked=false,
                CheckPoints = new List<CheckPoint>
                {
                    new CheckPointTransaction
                    {
                        Id="CP1",
                        Name="Filter Light indication",
                        Standard="Green Light",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    },
                    new CheckPointTransaction
                    {
                        Id="CP2",
                        Name="Air Filter Fan",
                        Standard="Running Condition",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    }
                }
            }
            };
            var checkPointBulkEntryDTO = new CheckPointEntryDTO
            {
                CheckSheetId = "cs2",
                CheckPointId = "CP1",
                CheckRecord = "OK",
                Comments = null,
                UserAction = "Submitted"
            };
            //_repositoryMock.Setup(repo => repo.GetByIdAsync(checkPointBulkEntryDTO.CheckSheetId)).ReturnsAsync((string id) => checksheetstransaction.FirstOrDefault(c => c.Id == id));

            //_cacheMock.Setup(c => c.Get<List<CheckSheetTransaction>>("CheckSheetTransaction")).Returns(checksheetstransaction);
            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
           
            //var result = await service.BulkUpdateCheckRecordAsync(checkPointBulkEntryDTO);
            //Assert.IsNotNull(result);
            //Assert.IsFalse(result.IsSuccess);
            //Assert.AreEqual($"Check sheet with id {checkPointBulkEntryDTO.CheckSheetId} not found", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task deleteAsync_ShouldReturnError_WhenCanDeleteIsFalse()
        {
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
            new CheckSheetTransaction
            {
                Id ="CS1",
                Name="Air Filter assembly",
                Line="R6",
                UniqueId="A1102",
                Equipment="Air Filter",
                EquipmentCode="EA0001",
                Station = "STA01",
                Location="FACTORY-II",
                SubLocation="EL Box Assembly",
                CheckSheetDay=DateTime.Parse("2024-02-02T00:00:00Z"),
                StartedBy="SYS",
                StartedOn=DateTime.Parse("2024-02-02T00:00:00Z"),
                Status="Yet to Start",
                ColorCode="Black",
                IsActive=true,
                IsLocked=false,
                CheckPoints = new List<CheckPoint>
                {
                    new CheckPointTransaction
                    {
                        Id="CP1",
                        Name="Filter Light indication",
                        Standard="Green Light",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    },
                    new CheckPointTransaction
                    {
                        Id="CP2",
                        Name="Air Filter Fan",
                        Standard="Running Condition",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    }
                }
            }
            };
            //_repositoryMock.Setup(repo => repo.GetByIdAsync(checksheetstransaction[0].Id)).ReturnsAsync((string id) => checksheetstransaction.FirstOrDefault(c => c.Id == id));
            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
            //var result = await service.DeleteAsync(checksheetstransaction[0].Id);
            //Assert.IsNotNull(result);
            //Assert.IsTrue(result.IsSuccess);
            //Assert.AreEqual("CheckSheetTransaction deleted", result.Messages.ToList()[0]);


        }
        [TestMethod]
        public async Task deleteAsync_ShouldReturnError_WhenIdIsInvalid()
        {
            
            var checksheetstransaction = new List<CheckSheetTransaction>
            {
            new CheckSheetTransaction
            {
                Id ="CS1",
                Name="Air Filter assembly",
                Line="R6",
                UniqueId="A1102",
                Equipment="Air Filter",
                EquipmentCode="EA0001",
                Station = "STA01",
                Location="FACTORY-II",
                SubLocation="EL Box Assembly",
                CheckSheetDay=DateTime.Parse("2024-02-02T00:00:00Z"),
                StartedBy="SYS",
                StartedOn=DateTime.Parse("2024-02-02T00:00:00Z"),
                Status="Yet to Start",
                ColorCode="Black",
                IsActive=true,
                IsLocked=false,
                CheckPoints = new List<CheckPoint>
                {
                    new CheckPointTransaction
                    {
                        Id="CP1",
                        Name="Filter Light indication",
                        Standard="Green Light",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    },
                    new CheckPointTransaction
                    {
                        Id="CP2",
                        Name="Air Filter Fan",
                        Standard="Running Condition",
                        Method="Visual/Hand",
                        SeqOrder=1,
                        FrequencyType="daily",
                        CheckRecord="OK",
                        Comments="",
                        IsForToday=true,
                    }
                }
            }
            };
            //_repositoryMock.Setup(repo => repo.GetByIdAsync("CS2")).ReturnsAsync((string id) => checksheetstransaction.FirstOrDefault(c => c.Id == id));
            //var service = new CheckSheetTransactionService(_repositoryMock.Object, _mapperMock.Object, _imageService.Object, _logger.Object);
            //var result = await service.DeleteAsync("CS2");
            //Assert.IsNotNull(result);
            //Assert.IsFalse(result.IsSuccess);
            //Assert.AreEqual("Invalid id", result.Messages.ToList()[0]);


        }

    }
}
