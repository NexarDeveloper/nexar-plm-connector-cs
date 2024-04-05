using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using CustomPLMService.Contract.Models.Search;
using FilesystemPLMDriver;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using CustomPLMService.Contract.Models.Authentication;
using Microsoft.Extensions.Logging;
using Type = CustomPLMService.Contract.Models.Metadata.Type;

namespace CustomPLMService.Tests
{
    public class FileSystemServiceTest : IDisposable
    {
        private readonly string serviceDir;
        private readonly FileSystemPlmService service;
        private readonly FileSystemPlmMetadataService metadataService;

        public FileSystemServiceTest()
        {
            var metadataConfig = new MetadataConfig
            {
                AttributeNames = Array.Empty<string>(),
                ChangeTypes = new List<string>
                {
                    "ECO"
                },
                ItemTypes = new List<string>
                {
                    "Capacitor",
                    "Resistor"
                }
            };
            var metadataConfigMock = new Mock<IOptions<MetadataConfig>>();
            metadataConfigMock.SetupGet(m => m.Value).Returns(metadataConfig);
            
            var userContextMock = new Mock<IContext>();
            var loggerMock = new Mock<ILogger<FileSystemPlmService>>();
            
            serviceDir = Path.Combine(Environment.CurrentDirectory, "testRepo");
            service = new FileSystemPlmService(new ItemRepository(serviceDir), loggerMock.Object);
            metadataService = new FileSystemPlmMetadataService(metadataConfigMock.Object, userContextMock.Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(serviceDir))
            {
                Directory.Delete(serviceDir, true);
            }
        }

        [Fact]
        public async Task ShouldCreateAndReadItem()
        {
            var (capacitorId, capacitor, created) = await CreateCapacitor();

            Assert.NotNull(created.Id);
            Assert.Equal(capacitorId, created.Id.TypeId);
            Assert.NotNull(created.Id.PrivateId);
            Assert.StartsWith("PLM-FS-", created.Id.PublicId);

            Assert.Equal(2, created.Values.Count);
            Assert.Equal("nameValue", StringAttribute(created, "name"));
            Assert.Equal("descriptionValue", StringAttribute(created, "description"));

            // update item
            var updateSpec = new ItemUpdateSpec
            {
                Id = created.Id,
                Metadata = capacitor
            };
            updateSpec.Values.Add(StringAttribute("name", "nameValue"));
            updateSpec.Values.Add(StringAttribute("description", "descriptionUpdated"));

            var updated = await service.UpdateItem(updateSpec);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal(2, updated.Values.Count);
            Assert.Equal("nameValue", StringAttribute(updated, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(updated, "description"));

            // read item
            var item = await service.ReadItem(created.Id);
            Assert.Equal(created.Id, item.Id);
            Assert.Equal(2, item.Values.Count);
            Assert.Equal("nameValue", StringAttribute(item, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(item, "description"));
        }

        [Fact]
        public async Task ShouldCreateAndReadChange()
        {
            var ecoId = new TypeId
            {
                ApiName = "ECO",
                Name = "ECO",
                Id = "ECO",
                BaseType = BaseType.Change
            };
            var eco = (await metadataService.ReadTypes(SingletonList(ecoId))).First();
            var createSpec = new ItemCreateSpec
            {
                Metadata = eco,
            };
            createSpec.Values.Add(StringAttribute("name", "nameValue"));
            createSpec.Values.Add(StringAttribute("description", "descriptionValue"));
            var created = await service.CreateItem(createSpec);
            Assert.NotNull(created.Id);
            Assert.Equal(ecoId, created.Id.TypeId);
            Assert.NotNull(created.Id.PrivateId);
            Assert.StartsWith("PLM-FS-ECO-", created.Id.PublicId);

            Assert.Equal(2, created.Values.Count);
            Assert.Equal("nameValue", StringAttribute(created, "name"));
            Assert.Equal("descriptionValue", StringAttribute(created, "description"));

            // update item
            var updateSpec = new ItemUpdateSpec
            {
                Id = created.Id,
                Metadata = eco
            };
            updateSpec.Values.Add(StringAttribute("name", "nameValue"));
            updateSpec.Values.Add(StringAttribute("description", "descriptionUpdated"));

            var updated = await service.UpdateItem(updateSpec);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal(2, updated.Values.Count);
            Assert.Equal("nameValue", StringAttribute(updated, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(updated, "description"));

            // read item
            var item = await service.ReadItem(created.Id);
            Assert.Equal(created.Id, item.Id);
            Assert.Equal(2, item.Values.Count);
            Assert.Equal("nameValue", StringAttribute(item, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(item, "description"));
        }

        private async Task<(TypeId capacitorId, Type capacitor, Item created)> CreateCapacitor()
        {
            var capacitorId = new TypeId
            {
                ApiName = "Capacitor",
                Name = "Capacitor",
                Id = "Capacitor"
            };
            var capacitor = (await metadataService.ReadTypes(new List<TypeId>
            {
                capacitorId
            })).First();
            var createSpec = new ItemCreateSpec
            {
                Metadata = capacitor,
            };
            createSpec.Values.Add(StringAttribute("name", "nameValue"));
            createSpec.Values.Add(StringAttribute("description", "descriptionValue"));
            var created = await service.CreateItem(createSpec);

            return (capacitorId, capacitor, created);
        }

        [Fact]
        public async Task ShouldReturnNullForNotExistingItem()
        {
            var plmId = new Id
            {
                PublicId = "xxxx",
                TypeId = new TypeId
                {
                    ApiName = "Capacitor",
                    Name = "Capacitor",
                    Id = "Capacitor"
                }
            };
            var result = await service.ReadItem(plmId);

            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldFindAllMatchingItems()
        {
            const int count = 5;
            var capacitorId = new TypeId
            {
                ApiName = "Capacitor",
                Name = "Capacitor",
                Id = "Capacitor"
            };

            var resistorId = new TypeId
            {
                ApiName = "Resistor",
                Name = "Resistor",
                Id = "Resistor"
            };
            var capacitor = (await metadataService.ReadTypes(SingletonList(capacitorId))).First();
            var resistor = (await metadataService.ReadTypes(SingletonList(resistorId))).First();

            for (var i = 0; i < count; i++)
            {
                var matchingSpec = new ItemCreateSpec
                {
                    Metadata = capacitor,
                };
                matchingSpec.Values.Add(StringAttribute("x", "y"));
                await service.CreateItem(matchingSpec);
            }

            var wrongTypeSpec = new ItemCreateSpec
            {
                Metadata = resistor,
            };
            await service.CreateItem(wrongTypeSpec);

            var wrongAttributeSpec = new ItemCreateSpec
            {
                Metadata = capacitor,
            };
            wrongAttributeSpec.Values.Add(StringAttribute("x", "z"));
            await service.CreateItem(wrongAttributeSpec);

            var q = new Query
            {
                Type = "Capacitor"
            };
            var att = new QueryAttribute
            {
                Name = "x",
                Value = "y"
            };
            q.Attrs.Add(att);
            var found = await service.QueryItems(q, capacitor);
            Assert.True(count == found.Count());
        }

        private static AttributeValue StringAttribute(string name, string value)
        {
            var attribute = new AttributeValue
            {
                AttributeId = name,
                Value = new Value(value)
            };
            return attribute;
        }

        private static string StringAttribute(Item item, string name)
        {
            return item.Values.First(attribute => attribute.AttributeId == name).Value;
        }

        private static IList<T> SingletonList<T>(T item)
        {
            return new List<T>
            {
                item
            };
        }
    }
}
