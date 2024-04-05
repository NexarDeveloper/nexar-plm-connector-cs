using System;
using System.Collections.Generic;
using System.Linq;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using CustomPLMService.Contract.Models.Relationship;
using FilesystemPLMDriver.Models;
using Type = CustomPLMService.Contract.Models.Metadata.Type;

namespace FilesystemPLMDriver
{
    internal static class DtoConverter
    {
        public static IdDto ToIdDto(this Id plmId, Type plmType)
        {
            return new IdDto
            {
                AlternateId = plmId?.PublicId,
                Id = plmId?.PrivateId,
                Type = plmId?.TypeId.Id ?? plmType.Id.Id,
            };
        }

        public static Id ToPlmId(IdDto idDto, bool changeType)
        {
            return new Id
            {
                PrivateId = idDto.Id,
                PublicId = idDto.AlternateId,
                TypeId = new TypeId
                {
                    Id = idDto.Type,
                    ApiName = idDto.Type,
                    Name = idDto.Type,
                    BaseType = changeType ? BaseType.Change : BaseType.Item
                }
            };
        }

        public static ObjectDto ToObjectDto(ItemCreateSpec createSpec)
        {
            var itemDto = new ObjectDto
            {
                Id = ToIdDto(null, createSpec.Metadata)
            };
            PopulateAttributes(createSpec.Values, itemDto);
            return itemDto;
        }

        public static ObjectDto ToObjectDto(ItemUpdateSpec spec)
        {
            var itemDto = new ObjectDto
            {
                Id = ToIdDto(spec.Id, spec.Metadata)
            };
            PopulateAttributes(spec.Values, itemDto);
            return itemDto;
        }

        public static ItemDto ToItemDto(ItemUpdateSpec updateSpec)
        {
            var dto = ToObjectDto(updateSpec);
            var itemDto = new ItemDto(dto);
            PopulateItemAttributes(updateSpec.Values, itemDto);
            return itemDto;
        }

        public static ItemDto ToItemDto(ItemCreateSpec spec)
        {
            var dto = ToObjectDto(spec);
            var itemDto = new ItemDto(dto);
            PopulateItemAttributes(spec.Values, itemDto);
            return itemDto;
        }

        public static ChangeDto ToChangeDto(ItemUpdateSpec updateSpec)
        {
            var dto = ToObjectDto(updateSpec);
            var changeDto = new ChangeDto(dto);
            return changeDto;
        }

        public static ChangeDto ToChangeDto(ItemCreateSpec spec)
        {
            var dto = ToObjectDto(spec);
            var changeDto = new ChangeDto(dto);
            return changeDto;
        }

        public static Item ToPlmItem(ObjectDto itemDto)
        {
            var plmItem = new Item
            {
                Id = ToPlmId(itemDto.Id, !(itemDto is ItemDto))
            };
            var attributes = itemDto.Attributes.Select(ToPlmAttributeValue).ToList();
            plmItem.Values.AddRange(attributes);
            return plmItem;
        }

        private static AttributeValue ToPlmAttributeValue(AttributeValueDto attribute)
        {
            var plmAttributeValue = new AttributeValue
            {
                AttributeId = attribute.Name,
                Value = new Value(attribute.Value)
            };
            return plmAttributeValue;
        }

        private static void PopulateAttributes(IEnumerable<AttributeValue> values, ObjectDto itemDto)
        {
            var attributes = values.Select(attribute => new AttributeValueDto
            {
                Name = attribute.AttributeId,
                Value = attribute.Value
            });
            itemDto.Attributes.AddRange(attributes);
        }

        private static void PopulateItemAttributes(IEnumerable<AttributeValue> values, ItemDto itemDto)
        {
            var nameAttribute = values.FirstOrDefault(attribute => attribute.AttributeId.ToLower() == "name");
            if (nameAttribute != null)
            {
                itemDto.Name = nameAttribute.Value;
            }

            itemDto.ModifyDate = DateTime.Now.Ticks;
        }

        public static RelationshipTableDto ToRelationshipTableDto(RelationshipTable table)
        {
            var relationshipTable = new RelationshipTableDto()
            {
                Type = ConvertRelationshipType(table.Type)
            };

            var rows = table.Rows.Select(ToRelationshipDto).ToList();
            relationshipTable.Rows.AddRange(rows);

            return relationshipTable;
        }

        private static RelationshipDto ToRelationshipDto(RelationshipRow relationship)
        {
            var dto = new RelationshipDto()
            {
                RelationshipId = relationship.Id
            };

            if (relationship.ChildId != null)
            {
                dto.ChildId = ToIdDto(relationship.ChildId, null);
            }
            else
            {
                dto.SourceFile = relationship.FileResource;
            }

            var attributes = relationship.Attributes.Select(attribute => new AttributeValueDto
            {
                Name = attribute.AttributeId,
                Value = attribute.Value
            }).ToList();
            dto.Attributes.AddRange(attributes);

            return dto;
        }

        private static string ConvertRelationshipType(RelationshipType type)
        {
            return type switch
            {
                RelationshipType.Bom => "BOM",
                RelationshipType.Attachments => "ATTACHMENTS",
                RelationshipType.ManufacturerParts => "MANUFACTURER_PARTS",
                RelationshipType.AffectedItems => "AFFECTED_ITEMS",
                _ => throw new Exception("Unsupported relation type")
            };
        }
    }
}
