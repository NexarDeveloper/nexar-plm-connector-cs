using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using CustomPLMService.Contract.Models;
using CustomPLMService.Contract.Models.Items;
using FluentAssertions;
using Google.Protobuf;
using Xunit;
using Assert = Xunit.Assert;
using BaseTypeTO = Altium.PLM.Custom.BaseType;
using IdTO = Altium.PLM.Custom.Id;
using ItemTO = Altium.PLM.Custom.Item;
using TypeIdTO = Altium.PLM.Custom.TypeId;
using AttributeValueTO = Altium.PLM.Custom.AttributeValue;
using ValueTO = Altium.PLM.Custom.Value;
using UomValueTO = Altium.PLM.Custom.UomValue;
using ListValueTO = Altium.PLM.Custom.ListValue;
using OperationTo = Altium.PLM.Custom.OperationSupportedRequest.Types.Operation;
using FileResourceTO = Altium.PLM.Custom.FileResource;

namespace CustomPLMService.Tests
{
    [ExcludeFromCodeCoverage]
    public class MappingTest
    {
        private readonly Mapper mapper;

        public MappingTest()
        {
            var mappingProfile = new PlmServiceMappingProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(mappingProfile));
            mapper = new Mapper(configuration);
        }

        [Fact]
        public void SupportedOperationTest()
        {
            var changeOrderOperation = mapper.Map<SupportedOperation>(OperationTo.CreateChangeOrder);
            Assert.Equal(SupportedOperation.CreateChangeOrder, changeOrderOperation);

            var partChoicesFromAttributesOperation = mapper.Map<SupportedOperation>(OperationTo.ExtractPartChoicesFromAttributes);
            Assert.Equal(SupportedOperation.ExtractPartChoicesFromAttributes, partChoicesFromAttributesOperation);
        }

        [Fact]
        public void ItemMappingTest()
        {
            // Arrange
            var grpcItem = new ItemTO
            {
                Id = new IdTO
                {
                    PublicId = "PublicId",
                    PrivateId = "PrivateId",
                    TypeId = new TypeIdTO
                    {
                        Id = "TypeId",
                        ApiName = "TypeApiName",
                        BaseType = BaseTypeTO.Item
                    }
                }
            };

            var stringAttribute = new AttributeValueTO
            {
                AttributeId = "stringAttribute"
            };
            stringAttribute.Value.Add(new ValueTO
            {
                StringValue = "stringValue"
            });
            grpcItem.Values.Add(stringAttribute);

            var boolAttribute = new AttributeValueTO
            {
                AttributeId = "boolAttribute"
            };
            boolAttribute.Value.Add(new ValueTO
            {
                BoolValue = true
            });
            grpcItem.Values.Add(boolAttribute);

            var dateAttribute = new AttributeValueTO
            {
                AttributeId = "dateAttribute"
            };
            dateAttribute.Value.Add(new ValueTO
            {
                DateValue = 1999
            });
            grpcItem.Values.Add(dateAttribute);

            var doubleAttribute = new AttributeValueTO
            {
                AttributeId = "doubleAttribute"
            };
            doubleAttribute.Value.Add(new ValueTO
            {
                DoubleValue = 99.99
            });
            grpcItem.Values.Add(doubleAttribute);

            var floatAttribute = new AttributeValueTO
            {
                AttributeId = "floatAttribute"
            };
            floatAttribute.Value.Add(new ValueTO
            {
                FloatValue = 99.99f
            });
            grpcItem.Values.Add(floatAttribute);

            var intAttribute = new AttributeValueTO
            {
                AttributeId = "intAttribute"
            };
            intAttribute.Value.Add(new ValueTO
            {
                IntValue = 42
            });
            grpcItem.Values.Add(intAttribute);

            var referenceAttribute = new AttributeValueTO
            {
                AttributeId = "referenceAttribute"
            };
            referenceAttribute.Value.Add(new ValueTO
            {
                ReferenceValue = new IdTO
                {
                    PrivateId = "referencePrivateId",
                    PublicId = "referencePublicId",
                    TypeId = new TypeIdTO
                    {
                        ApiName = "referenceTypeApiName",
                        Id = "referenceTypeId",
                        Name = "referenceType",
                        BaseType = BaseTypeTO.Change
                    }
                }
            });
            grpcItem.Values.Add(referenceAttribute);

            var uomAttribute = new AttributeValueTO
            {
                AttributeId = "uomAttribute"
            };
            uomAttribute.Value.Add(new ValueTO
            {
                UomValue = new UomValueTO
                {
                    UnitName = "SomeUnit",
                    UnitValue = 99.99
                },
            });
            grpcItem.Values.Add(uomAttribute);

            var listValueAttribute = new AttributeValueTO
            {
                AttributeId = "listValueAttribute"
            };
            listValueAttribute.Value.Add(new ValueTO
            {
                ListValue = new ListValueTO
                {
                    Id = "id",
                    Value = new ValueTO
                    {
                        UomValue = new UomValueTO
                        {
                            UnitName = "Unit",
                            UnitValue = 33.33
                        }
                    }
                }
            });
            grpcItem.Values.Add(listValueAttribute);

            // Act
            var item = mapper.Map<ItemTO>(grpcItem);
            var backToGrpc = mapper.Map<ItemTO>(item);
            
            // Arrange
            backToGrpc.Should().BeEquivalentTo(grpcItem);

            item.Id.PrivateId.Should().Be(grpcItem.Id.PrivateId);
            item.Id.PublicId.Should().Be(grpcItem.Id.PublicId);
            item.Id.TypeId.Id.Should().Be(grpcItem.Id.TypeId.Id);
            item.Id.TypeId.Name.Should().Be(grpcItem.Id.TypeId.Name);
            item.Id.TypeId.ApiName.Should().Be(grpcItem.Id.TypeId.ApiName);
            item.Id.TypeId.BaseType.ToString().Should().Be(grpcItem.Id.TypeId.BaseType.ToString());
        }

        [Fact]
        public void FileRequestMappingTest()
        {
            // Arrange
            const string fileName = "testFileName";
            var data = new byte[]
            {
                1, 2, 3
            };

            var fileResource = new FileResourceTO()
            {
                FileName = fileName,
                Data = ByteString.CopyFrom(data)
            };

            // Act
            var mappedFileResource = mapper.Map<FileResource>(fileResource);

            // Assert
            mappedFileResource.Should().NotBeNull();
            mappedFileResource.FileName.Should().Be(fileName);
            mappedFileResource.Data.Length.Should().Be(data.Length);

            mappedFileResource.Data.ToArray().Should().BeEquivalentTo(data);
        }

        [Fact]
        public void ItemResultMapping_ItemProvided()
        {
            // Arrange
            var itemResult = new ItemResult
            {
                Item = new Item
                {
                    Id = new Id
                    {
                        PublicId = "testID"
                    }
                }
            };

            // Act
            var mappedItemResult = mapper.Map<Altium.PLM.Custom.ItemResult>(itemResult);

            // Assert
            mappedItemResult?.Item.Should().NotBeNull();
            mappedItemResult?.Item?.Id?.PublicId.Should().Be(itemResult.Item.Id.PublicId);
            mappedItemResult?.Error.Should().BeNull();
        }

        [Fact]
        public void ItemResultMapping_ErrorProvided()
        {
            // Arrange
            var itemResult = new ItemResult
            {
                ErrorMessage = "SomeTestErrorMessage"
            };

            // Act
            var mappedItemResult = mapper.Map<Altium.PLM.Custom.ItemResult>(itemResult);

            // Assert
            mappedItemResult?.Item.Should().BeNull();
            mappedItemResult?.Error.Should().NotBeNull();
            mappedItemResult?.Error.Message.Should().Be(itemResult.ErrorMessage);
        }
    }
}
