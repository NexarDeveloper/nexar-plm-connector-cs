using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models;

public class IdDto
{
    [XmlElement("id")]
    public string Id { get; set; }

    [XmlElement("alternateId")]
    public string AlternateId { get; set; }

    [XmlElement("type")]
    public string Type { get; set; }
}
