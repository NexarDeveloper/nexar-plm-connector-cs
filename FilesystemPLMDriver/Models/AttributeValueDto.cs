using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models;

[ExcludeFromCodeCoverage]
public class AttributeValueDto
{
    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("value")]
    public string Value { get; set; }
}
