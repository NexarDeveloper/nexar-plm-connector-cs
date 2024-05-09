using AutoMapper;
using CustomPLMService.Contract.Models;
using CustomPLMService.Contract.Models.Items;
using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class MappingTest
    {
        private readonly IMapper mapper;

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

            var item = mapper.Map<ItemTO>(grpcItem);

            var backToGrpc = mapper.Map<ItemTO>(item);
            Assert.Equal(grpcItem, backToGrpc);

            Assert.Equal(grpcItem.Id.PrivateId, item.Id.PrivateId);
            Assert.Equal(grpcItem.Id.PublicId, item.Id.PublicId);
            Assert.Equal(grpcItem.Id.TypeId.Id, item.Id.TypeId.Id);
            Assert.Equal(grpcItem.Id.TypeId.Name, item.Id.TypeId.Name);
            Assert.Equal(grpcItem.Id.TypeId.ApiName, item.Id.TypeId.ApiName);
            Assert.Equal(grpcItem.Id.TypeId.BaseType.ToString(), item.Id.TypeId.BaseType.ToString());
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
            Assert.NotNull(mappedFileResource);
            Assert.Equal(fileName, mappedFileResource.FileName);
            Assert.Equal(data.Length, mappedFileResource.Data.Length);

            CollectionAssert.AreEqual(data, mappedFileResource.Data.ToArray());
        }
    }
}
