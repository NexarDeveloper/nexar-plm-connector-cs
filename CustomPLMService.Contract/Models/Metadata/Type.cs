using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Metadata;

/// <summary>
/// Represents object type in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class Type
{
    /// <summary>
    /// Object type identifier (see <see cref="TypeId"/>)
    /// </summary>
    public TypeId Id { get; set; }

    /// <summary>
    /// Object type attribute specifications (see <see cref="AttributeSpec"/>)
    /// </summary>
    public List<AttributeSpec> Attributes { get; set; } = new List<AttributeSpec>();

    /// <summary>
    /// Object type relationship specifications (see <see cref="RelationshipSpec"/>)
    /// </summary>
    public List<RelationshipSpec> Relationships { get; set; } = new List<RelationshipSpec>();
}
