using System.Collections.Generic;
namespace CustomPLMService.Contract.Models.Metadata;

/// <summary>
/// Represents relationship specification in external system.
/// </summary>
public class RelationshipSpec
{
    /// <summary>
    /// Relationship type (see <see cref="RelationshipType"/>)
    /// </summary>
    public RelationshipType Type { get; set; }

    /// <summary>
    /// Relationship attribute specifications (see <see cref="AttributeSpec"/>)
    /// </summary>
    public List<AttributeSpec> Attributes { get; set; } = new List<AttributeSpec>();
}
