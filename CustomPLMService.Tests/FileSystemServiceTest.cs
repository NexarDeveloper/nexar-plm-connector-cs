using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using FilesystemPLMDriver;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.Contract.Models.Query;
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

            var result = await service.UpdateItems(new[] { updateSpec }, CancellationToken.None);
            var updated = result.First();
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal(2, updated.Values.Count);
            Assert.Equal("nameValue", StringAttribute(updated, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(updated, "description"));

            // read item
            var items = await service.ReadItems(new[] { created.Id }, CancellationToken.None);
            var item = items.First();
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
            var eco = (await metadataService.ReadTypes(new[] { ecoId })).First();
            var createSpec = new ItemCreateSpec
            {
                Metadata = eco,
            };
            createSpec.Values.Add(StringAttribute("name", "nameValue"));
            createSpec.Values.Add(StringAttribute("description", "descriptionValue"));
            var createResult = await service.CreateItems(new[] { createSpec }, CancellationToken.None);
            var created = createResult.First();
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

            var updateResult = await service.UpdateItems(new[] { updateSpec }, CancellationToken.None);
            var updated = updateResult.First();
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal(2, updated.Values.Count);
            Assert.Equal("nameValue", StringAttribute(updated, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(updated, "description"));

            // read item
            var readResult = await service.ReadItems(new[] { created.Id }, CancellationToken.None);
            var item = readResult.First();
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
            var created = await service.CreateItems(new[] { createSpec }, CancellationToken.None);

            return (capacitorId, capacitor, created.First());
        }

        [Fact]
        public async Task ShouldNotReturnAnythingForNotExistingItem()
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
            var result = await service.ReadItems(new[] { plmId }, CancellationToken.None);

            Assert.Empty(result);
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
            var capacitor = (await metadataService.ReadTypes(new[] { capacitorId })).First();
            var resistor = (await metadataService.ReadTypes(new[] { resistorId })).First();

            var matchingSpecs = new List<ItemCreateSpec>();
            for (var i = 0; i < count; i++)
            {
                var matchingSpec = new ItemCreateSpec
                {
                    Metadata = capacitor,
                };
                matchingSpec.Values.Add(StringAttribute("x", "y"));
                matchingSpecs.Add(matchingSpec);
            }

            await service.CreateItems(matchingSpecs, CancellationToken.None);

            var wrongTypeSpec = new ItemCreateSpec
            {
                Metadata = resistor,
            };
            await service.CreateItems(new[] { wrongTypeSpec }, CancellationToken.None);

            var wrongAttributeSpec = new ItemCreateSpec
            {
                Metadata = capacitor,
            };
            wrongAttributeSpec.Values.Add(StringAttribute("x", "z"));
            await service.CreateItems(new[] { wrongAttributeSpec }, CancellationToken.None);

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
            var found = await service.QueryItems(q, capacitor, CancellationToken.None);
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
    }
}