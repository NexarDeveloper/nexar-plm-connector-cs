using CustomPLMService.Contract;
using CustomPLMService.Contract.Models;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.Contract.Models.Metadata;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace FilesystemPLMDriver
{
    public class FileSystemPlmMetadataService : ICustomPlmMetadataService
    {
        private readonly IReadOnlyCollection<TypeId> typesIds;
        private readonly IReadOnlyCollection<TypeId> changeTypesIds;
        private readonly IReadOnlyList<AttributeSpec> attributes;
        private readonly IReadOnlyList<RelationshipSpec> relationships;

        private readonly IContext userContext;

        public FileSystemPlmMetadataService(IOptions<MetadataConfig> config, IContext userContext)
        {
            typesIds = config.Value.ItemTypes.Select(name => CreateTypeId(name, BaseType.Item)).ToList();
            changeTypesIds = config.Value.ChangeTypes.Select(name => CreateTypeId(name, BaseType.Change)).ToList();
            attributes = config.Value.AttributeNames.Select(CreatePlmAttributeSpec).ToList();
            relationships = CreatePlmRelationshipSpecs(attributes).ToList();

            this.userContext = userContext;
        }

        public Task<IEnumerable<TypeId>> ReadTypeIdentifiers(BaseType baseType)
        {
            return Task.FromResult(baseType switch
            {
                BaseType.Item => typesIds,
                BaseType.Change => changeTypesIds,
                _ => Enumerable.Empty<TypeId>()
            });
        }

        public Task<IEnumerable<Type>> ReadTypes(IEnumerable<TypeId> typeId)
        {
            return Task.FromResult(typeId.Select(id =>
            {
                var plmType = new Type
                {
                    Id = id
                };

                plmType.Attributes.AddRange(attributes);
                plmType.Relationships.AddRange(relationships);
                return plmType;
            }));
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
                ListValues = [],
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
