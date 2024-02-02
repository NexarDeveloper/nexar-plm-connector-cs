using System.Collections.Generic;
using System.Linq;
using CustomPLMService.Contract;

namespace CustomPLMDriver
{
    public class FileSystemPlmMetadataService : ICustomPlmMetadataService
    {
        private readonly IReadOnlyCollection<TypeId> typesIds;
        private readonly IReadOnlyCollection<TypeId> changeTypesIds;
        private readonly IReadOnlyList<AttributeSpec> attributes;
        private readonly IReadOnlyList<RelationshipSpec> relationships;

        public FileSystemPlmMetadataService(MetadataConfig config)
        {
            typesIds = config.ItemTypes.Select(name => CreateTypeId(name, BaseType.Item)).ToList();
            changeTypesIds = config.ChangeTypes.Select(name => CreateTypeId(name, BaseType.Change)).ToList();
            attributes = config.AttributeNames.Select(CreatePlmAttributeSpec).ToList();
            relationships = CreatePlmRelationshipSpecs(attributes).ToList();
        }

        public IEnumerable<TypeId> ReadTypeIdentifiers(Context context, BaseType baseType)
        {
            switch (baseType)
            {
                case BaseType.Item:
                    return typesIds;
                case BaseType.Change:
                    return changeTypesIds;
                default:
                    return Enumerable.Empty<TypeId>();
            }
        }

        public IEnumerable<Type> ReadTypes(Context context, IEnumerable<TypeId> typeId)
        {
            return typeId.Select(id =>
            {
                var plmType = new Type
                {
                    Id = id
                };

                plmType.Attributes.AddRange(attributes);
                plmType.Relationships.AddRange(relationships);
                return plmType;
            });
        }

        private static TypeId CreateTypeId(string typeName, BaseType baseType)
        {
            return new TypeId
            {
                ApiName = typeName,
                Id = typeName,
                Name = typeName,
                BaseType = baseType
            };
        }

        private static AttributeSpec CreatePlmAttributeSpec(string attributeName)
        {
            return new AttributeSpec
            {
                Id = attributeName,
                Name = attributeName,
                Category = attributeName,
                MultiValued = false,
                ReadOnly = false,
                Required = false,
                UomFamilyName = attributeName,
                ListValues = new List<ListValue>(),
                DataType = AttributeSpec.Datatype.Text,
                ValuesetType = AttributeSpec.Valueset.Free
            };
        }

        private static IEnumerable<RelationshipSpec> CreatePlmRelationshipSpecs(IEnumerable<AttributeSpec> attributes)
        {
            var bom = new RelationshipSpec
            {
                Type = RelationshipType.Bom
            };
            bom.Attributes.AddRange(attributes);
            yield return bom;
        }
    }
}
