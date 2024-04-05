using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models;

public class AttributeValueDto
{
    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("value")]
    public string Value { get; set; }
}
