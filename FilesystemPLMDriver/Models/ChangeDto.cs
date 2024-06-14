using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
namespace FilesystemPLMDriver.Models
{
    [ExcludeFromCodeCoverage]
    [XmlRoot("change")]
    public class ChangeDto(ObjectDto o) : ObjectDto(o.Id, o.Attributes, o.RelationshipTables);
}
