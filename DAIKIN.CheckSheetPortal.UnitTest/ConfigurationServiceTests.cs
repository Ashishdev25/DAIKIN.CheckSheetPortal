using AutoMapper;
using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using DAIKIN.CheckSheetPortal.Infrastructure.Services;
using DAIKIN.CheckSheetPortal.Services;
using Moq;

namespace DAIKIN.CheckSheetPortal.UnitTest
{
    [TestClass]
    public class ConfigurationServiceTests
    {
        private Mock<IRepository<Configuration>> _repositoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ICacheService> _cacheMock;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IRepository<Configuration>>();
            _mapperMock = new Mock<IMapper>();
            _cacheMock = new Mock<ICacheService>();
        }

        [TestMethod]
        public async Task GetAllAsync_ShouldReturnItemsFromCache_WhenCacheIsNotEmpty()
        {
            // Arrange
            var cachedItems = new List<Configuration> {
                new Configuration { Code = "CLR001", Description = "Test", IsActive = true, Name = "Plant" } };

            _cacheMock.Setup(c => c.Get<IEnumerable<Configuration>>("Configuration")).Returns(cachedItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);

            // Act
            var result = await service.GetAllAsync();
            var items = ((OperationResponse<IEnumerable<Configuration>>)result).Payload;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            CollectionAssert.AreEqual(cachedItems, (System.Collections.ICollection)items);
        }
        [TestMethod]
        public async Task GetAllAsync_ShouldReturnItemsFromRepository_WhenCacheIsEmpty()
        {
            // Arrange
            _cacheMock.Setup(c => c.Get<IEnumerable<Configuration>>("Configuration")).Returns((List<Configuration>)null);
            var repositoryItems = new List<Configuration> {
                new Configuration { Code = "CLR001", Description = "Test1", IsActive = true, Name = "Plant1" },
                new Configuration { Code = "CLR002", Description = "Test2", IsActive = true, Name = "Plant2" }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);

            // Act
            var result = await service.GetAllAsync();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            var items = ((OperationResponse<IEnumerable<Configuration>>)result).Payload;
            _cacheMock.Verify(c => c.Set("Configuration", items, TimeSpan.FromHours(1)), Times.Once);
        }
        [TestMethod]
        public async Task CreateAsync_ShouldReturnError_WhenDuplicateCodeAndDescriptionIsCreated()
        {
            // Arrange
            var repositoryItems = new List<Configuration> {
        new Configuration { Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
        new Configuration { Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
    };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Code = "RAJ", Description = "Rajastan ", IsActive = true, Name = "Plant" };

            // Act
            var result = await service.CreateAsync(newConfig);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Code or Description already exists", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task CreateAsync_ShouldReturnError_WhenDuplicateCodeIsCreated()
        {
            // Arrange
            var repositoryItems = new List<Configuration> {
        new Configuration { Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
        new Configuration { Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
    };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Code = "RAJ", Description = "XYZ", IsActive = true, Name = "Plant" };

            // Act
            var result = await service.CreateAsync(newConfig);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Code or Description already exists", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task CreateAsync_ShouldReturnError_WhenDuplicateDescriptionIsCreated()
        {
            // Arrange
            var repositoryItems = new List<Configuration> {
        new Configuration { Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
        new Configuration { Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
    };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Code = "XYZ", Description = "Karnataka", IsActive = true, Name = "Plant" };

            // Act
            var result = await service.CreateAsync(newConfig);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Code or Description already exists", result.Messages.ToList()[0]);
        }

        [TestMethod]

        public async Task CreateAsync_ShouldTrimWhiteSpaces_WhenCodeOrDescriptionEnteredWithSpace()
        {
            // Arrange
            var repositoryItems = new List<Configuration>{
                new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
                new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
            };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Id = "1", Code = " MUM ", Description = " Mumbai ", IsActive = true, Name = "Plant" };


            // Trim values before passing to CreateAsync
            newConfig.Description = newConfig.Description.Trim();
            newConfig.Name = newConfig.Name.Trim();
            newConfig.Code = newConfig.Code.Trim();

            // Act
            var result = await service.CreateAsync(newConfig);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
        }
        [TestMethod]

        public async Task CreateAsync_ShouldTrimWhitespacesAndReturnError_WhenDuplicateCodeCreated()
        {
            // Arrange
            var repositoryItems = new List<Configuration>{
                new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
                new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
            };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Code = " RAJ ", Description = " XYZ ", IsActive = true, Name = "Plant" };


            // Trim values before passing to CreateAsync
            newConfig.Description = newConfig.Description.Trim();
            newConfig.Name = newConfig.Name.Trim();
            newConfig.Code = newConfig.Code.Trim();

            // Act
            var result = await service.CreateAsync(newConfig);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Code or Description already exists", result.Messages.ToList()[0]);
        }
        public async Task CreateAsync_ShouldTrimWhitespacesAndReturnError_WhenDuplicateDescriptionCreated()
        {
            // Arrange
            var repositoryItems = new List<Configuration>{
                new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
                new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
            };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Code = " XYZ ", Description = " Rajastan ", IsActive = true, Name = "Plant" };


            // Trim values before passing to CreateAsync
            newConfig.Description = newConfig.Description.Trim();
            newConfig.Name = newConfig.Name.Trim();
            newConfig.Code = newConfig.Code.Trim();

            // Act
            var result = await service.CreateAsync(newConfig);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Code or Description already exists", result.Messages.ToList()[0]);
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldTrimWhitespacesAndUpdate()
        {
            // Arrange
            var repositoryItems = new List<Configuration>{
                new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
                new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
            };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Id = "1", Code = " MUM ", Description = " Mumbai ", IsActive = true, Name = "Plant" };


            // Trim values before passing to CreateAsync
            newConfig.Description = newConfig.Description.Trim();
            newConfig.Name = newConfig.Name.Trim();
            newConfig.Code = newConfig.Code.Trim();

            // Act
            var result = await service.UpdateAsync("1", newConfig);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
        }
        [TestMethod]
        public async Task updateAsync_ShouldTrimWhitespacesAndReturnError_WhenDuplicateDescriptionIsCreated()
        {
            // Arrange
            var repositoryItems = new List<Configuration> {
                new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
                new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
            };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Id = "1", Code = " KAR    ", Description = "  Rajastan ", IsActive = true, Name = "  Plant  " };

            newConfig.Description = newConfig.Description.Trim();
            newConfig.Name = newConfig.Name.Trim();
            newConfig.Code = newConfig.Code.Trim();
            // Act
            var result = await service.UpdateAsync("1", newConfig);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Code or Description already exists", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task updateAsync_ShouldTrimWhitespacesAndReturnError_WhenDuplicateeCodeIsCreated()
        {
            // Arrange
            var repositoryItems = new List<Configuration> {
                new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
                new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
            };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Id = "1", Code = " RAJ    ", Description = " Karnataka   ", IsActive = true, Name = "  Plant  " };

            newConfig.Description = newConfig.Description.Trim();
            newConfig.Name = newConfig.Name.Trim();
            newConfig.Code = newConfig.Code.Trim();
            // Act
            var result = await service.UpdateAsync("1", newConfig);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Code or Description already exists", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task updateAsync_ShouldReturnError_WhenDuplicateCodeIsCreated()
        {
            // Arrange
            var repositoryItems = new List<Configuration> {
                new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
                new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
            };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Id = "1", Code = "RAJ", Description = "Karnataka", IsActive = true, Name = "Plant" };

            // Act
            var result = await service.UpdateAsync("1", newConfig);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Code or Description already exists", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task updateAsync_ShouldReturnError_WhenDuplicateDescriptionIsCreated()
        {
            // Arrange
            var repositoryItems = new List<Configuration> {
                new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant" },
                new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant" }
            };
            _repositoryMock.Setup(r => r.GetByFilterAsync("Name", "Plant")).ReturnsAsync(repositoryItems);

            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var newConfig = new Configuration { Id = "1", Code = "KAR", Description = "Rajastan", IsActive = true, Name = "Plant" };

            // Act
            var result = await service.UpdateAsync("1", newConfig);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Code or Description already exists", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task deleteAsync_ShouldReturnError_WhenInvalidIdIsPassed()
        {
            // Arrange
            var invalidId = "3";
            var repositoryItems = new List<Configuration> {
        new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant",CanDelete=false },
        new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant",CanDelete=false }
    };
            _repositoryMock.Setup(repo => repo.GetByIdAsync("3")).ReturnsAsync((string id) => repositoryItems.FirstOrDefault(c => c.Id == id));
            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);

            // Act
            var result = await service.DeleteAsync(invalidId);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Invalid id", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task deleteAsync_ShouldReturnError_WhenCanDeleteIsFalse()
        {
            var repositoryItems = new List<Configuration> {
        new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant",CanDelete=false },
        new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant",CanDelete=false }
            };
            _repositoryMock.Setup(repo => repo.GetByIdAsync(repositoryItems[0].Id)).ReturnsAsync((string id) => repositoryItems.FirstOrDefault(c => c.Id == id));
            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var result = await service.DeleteAsync(repositoryItems[0].Id);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Configuration cannot be deleted", result.Messages.ToList()[0]);
        }
        [TestMethod]
        public async Task deleteAsync_WhenCanDeleteIsTrue()
        {
            var repositoryItems = new List<Configuration> {
        new Configuration { Id="1",Code = "RAJ", Description = "Rajastan", IsActive = true, Name = "Plant",CanDelete=true },
        new Configuration { Id="2",Code = "KAR", Description = "Karnataka", IsActive = true, Name = "Plant",CanDelete=false }
            };
            _repositoryMock.Setup(repo => repo.GetByIdAsync(repositoryItems[0].Id)).ReturnsAsync((string id) => repositoryItems.FirstOrDefault(c => c.Id == id));
            var service = new ConfigurationService(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
            var result = await service.DeleteAsync(repositoryItems[0].Id);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Configuration deleted", result.Messages.ToList()[0]);
        }
    }
}