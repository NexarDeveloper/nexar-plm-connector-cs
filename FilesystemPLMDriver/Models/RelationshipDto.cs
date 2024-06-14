using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models;

[ExcludeFromCodeCoverage]
public class RelationshipDto
{
    [XmlElement("relationshipId")]
    public string RelationshipId { get; set; }
    [XmlElement("childId")]
    public IdDto ChildId { get; set; }
    [XmlElement("fileResource")]
    public string FileResource { get; set; }
    [XmlElement("attributes")]
    public List<AttributeValueDto> Attributes { get; set; } = [];
    [XmlIgnore]
    public string SourceFile { get; set; }
}
