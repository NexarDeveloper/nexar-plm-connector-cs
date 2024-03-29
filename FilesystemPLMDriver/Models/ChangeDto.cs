using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models
{

    [XmlRoot("change")]
    public class ChangeDto(ObjectDto o) : ObjectDto(o.Id, o.Attributes, o.RelationshipTables);
}
