using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models;

[ExcludeFromCodeCoverage]
public class IdDto
{
    [XmlElement("id")]
    public string Id { get; set; }

    [XmlElement("alternateId")]
    public string AlternateId { get; set; }

    [XmlElement("type")]
    public string Type { get; set; }
}
