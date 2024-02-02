using System.Collections.Generic;
using System.Xml.Serialization;

namespace CustomPLMDriver
{
    public class IdDto
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("alternateId")]
        public string AlternateId { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }
    }

    public class AttributeValueDto
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }
    }

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

    public class ObjectDto
    {
        public ObjectDto()
        {
            Attributes = new List<AttributeValueDto>();
            RelationshipTables = new List<RelationshipTableDto>();
        }

        public ObjectDto(IdDto id, List<AttributeValueDto> attributes, List<RelationshipTableDto> relationshipTables)
        {
            this.Id = id;
            this.Attributes = attributes;
            this.RelationshipTables = relationshipTables;
        }
        [XmlElement("id")]
        public IdDto Id { get; set; }

        [XmlElement("attributes")]
        public List<AttributeValueDto> Attributes { get; set; }
        [XmlElement("relationshipTables")]
        public List<RelationshipTableDto> RelationshipTables { get; set; }
    }

    public class RelationshipTableDto
    {
        public RelationshipTableDto()
        {
            Rows = new List<RelationshipDto>();
        }
        [XmlElement("type")]
        public string Type { get; set; }
        [XmlElement("rows")]
        public List<RelationshipDto> Rows { get; set; }
    }

    public class RelationshipDto
    {
        public RelationshipDto()
        {
            Attributes = new List<AttributeValueDto>();
        }
        [XmlElement("relationshipId")]
        public string RelationshipId { get; set; }
        [XmlElement("childId")]
        public IdDto ChildId { get; set; }
        [XmlElement("fileResource")]
        public string FileResource { get; set; }
        [XmlElement("attributes")]
        public List<AttributeValueDto> Attributes { get; set; }
        [XmlIgnore]
        public string SourceFile { get; set; }
    }

    [XmlRoot("change")]
    public class ChangeDto : ObjectDto
    {
        public ChangeDto(ObjectDto o) : base(o.Id, o.Attributes, o.RelationshipTables) { }
    }
}
