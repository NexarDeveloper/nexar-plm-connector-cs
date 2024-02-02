using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CustomPLMDriver;
using CustomPLMService.Contract;
using Type = CustomPLMService.Contract.Type;

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
                ChangeTypes = new List<string> { "ECO" },
                ItemTypes = new List<string> { "Capacitor", "Resistor" }
            };

            serviceDir = Path.Combine(Environment.CurrentDirectory, "testRepo");
            service = new FileSystemPlmService(new ItemRepository(serviceDir));
            metadataService = new FileSystemPlmMetadataService(metadataConfig);
        }

        public void Dispose()
        {
            if (Directory.Exists(serviceDir))
            {
                Directory.Delete(serviceDir, true);
            }
        }

        [Fact]
        public void ShouldCreateAndReadItem()
        {
            TypeId capacitorId;
            Type capacitor;
            Item created;
            CreateCapacitor(out capacitorId, out capacitor, out created);

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

            var updated = service.UpdateItem(null, updateSpec);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal(2, updated.Values.Count);
            Assert.Equal("nameValue", StringAttribute(updated, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(updated, "description"));

            // read item
            var item = service.ReadItem(null, created.Id);
            Assert.Equal(created.Id, item.Id);
            Assert.Equal(2, item.Values.Count);
            Assert.Equal("nameValue", StringAttribute(item, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(item, "description"));
        }

        [Fact]
        public void ShouldCreateAndReadChange()
        {
            var ecoId = new TypeId
            {
                ApiName = "ECO",
                Name = "ECO",
                Id = "ECO",
                BaseType = BaseType.Change
            };
            var eco = metadataService.ReadTypes(null, SingletonList(ecoId)).First();
            var createSpec = new ItemCreateSpec
            {
                Metadata = eco,
            };
            createSpec.Values.Add(StringAttribute("name", "nameValue"));
            createSpec.Values.Add(StringAttribute("description", "descriptionValue"));
            var created = service.CreateItem(null, createSpec);
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

            var updated = service.UpdateItem(null, updateSpec);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal(2, updated.Values.Count);
            Assert.Equal("nameValue", StringAttribute(updated, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(updated, "description"));

            // read item
            var item = service.ReadItem(null, created.Id);
            Assert.Equal(created.Id, item.Id);
            Assert.Equal(2, item.Values.Count);
            Assert.Equal("nameValue", StringAttribute(item, "name"));
            Assert.Equal("descriptionUpdated", StringAttribute(item, "description"));
        }

        private void CreateCapacitor(out TypeId capacitorId, out Type capacitor, out Item created)
        {
            capacitorId = new TypeId
            {
                ApiName = "Capacitor",
                Name = "Capacitor",
                Id = "Capacitor"
            };
            capacitor = metadataService.ReadTypes(null, new List<TypeId> {capacitorId}).First();
            var createSpec = new ItemCreateSpec
            {
                Metadata = capacitor,
            };
            createSpec.Values.Add(StringAttribute("name", "nameValue"));
            createSpec.Values.Add(StringAttribute("description", "descriptionValue"));
            created = service.CreateItem(null, createSpec);
        }

        [Fact]
        public void ShouldReturnNullForNotExistingItem()
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
            var result = service.ReadItem(null, plmId);

            Assert.Null(result);
        }

        [Fact]
        public void ShouldFindAllMatchingItems()
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
            var capacitor = metadataService.ReadTypes(null, SingletonList(capacitorId)).First();
            var resistor = metadataService.ReadTypes(null, SingletonList(resistorId)).First();

            for (var i = 0; i < count; i++)
            {
                var matchingSpec = new ItemCreateSpec
                {
                    Metadata = capacitor,
                };
                matchingSpec.Values.Add(StringAttribute("x", "y"));
                service.CreateItem(null, matchingSpec);
            }

            var wrongTypeSpec = new ItemCreateSpec
            {
                Metadata = resistor,
            };
            service.CreateItem(null, wrongTypeSpec);

            var wrongAttributeSpec = new ItemCreateSpec
            {
                Metadata = capacitor,
            };
            wrongAttributeSpec.Values.Add(StringAttribute("x", "z"));
            service.CreateItem(null, wrongAttributeSpec);

            var q = new Query {Type = "Capacitor"};
            var att = new QueryAttribute {Name = "x", Value = "y"};
            q.Attrs.Add(att);
            var found = service.QueryItems(null, q, capacitor);
            Assert.True(count == found.Count());
        }

        private static AttributeValue StringAttribute(string name, string value)
        {
            var attribute = new AttributeValue {AttributeId = name, Value = new Value(value)};
            return attribute;
        }

        private static string StringAttribute(Item item, string name)
        {
            return item.Values.AsEnumerable().First(attribute => attribute.AttributeId == name).Value;
        }

        private static IList<T> SingletonList<T>(T item)
        {
            return new List<T> {item};
        }
    }
}
