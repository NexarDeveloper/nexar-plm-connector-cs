using System.Collections.Generic;
using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models;

public class RelationshipTableDto
{
    [XmlElement("type")]
    public string Type { get; set; }
    [XmlElement("rows")]
    public List<RelationshipDto> Rows { get; set; } = [];
}
