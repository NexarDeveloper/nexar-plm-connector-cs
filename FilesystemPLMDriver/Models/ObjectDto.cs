using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models;

[ExcludeFromCodeCoverage]
public class ObjectDto(IdDto id, List<AttributeValueDto> attributes, List<RelationshipTableDto> relationshipTables)
{
    public ObjectDto() : this(null, [], [])
    {
    }

    [XmlElement("id")]
    public IdDto Id { get; set; } = id;

    [XmlElement("attributes")]
    public List<AttributeValueDto> Attributes { get; set; } = attributes;
    [XmlElement("relationshipTables")]
    public List<RelationshipTableDto> RelationshipTables { get; set; } = relationshipTables;
}
