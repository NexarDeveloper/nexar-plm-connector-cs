using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models;

[ExcludeFromCodeCoverage]
public class RelationshipTableDto
{
    [XmlElement("type")]
    public string Type { get; set; }
    [XmlElement("rows")]
    public List<RelationshipDto> Rows { get; set; } = [];
}
