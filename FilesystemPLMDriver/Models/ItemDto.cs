using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models;

[XmlRoot("item")]
public class ItemDto : ObjectDto
{
    public ItemDto() : base() { }
    public ItemDto(ObjectDto o) : base(o.Id, o.Attributes, o.RelationshipTables) { }
    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("modifyDate")]
    public long ModifyDate { get; set; }
}
