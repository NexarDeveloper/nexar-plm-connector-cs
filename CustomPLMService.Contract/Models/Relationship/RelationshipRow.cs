using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
namespace CustomPLMService.Contract.Models.Relationship;

/// <summary>
/// Represents object relationship in external system. Object relationship connects a pair of objects (parent and child).
/// There are different types of relationships (see <see cref="RelationshipType"/>). 
/// </summary>
[ExcludeFromCodeCoverage]
public class RelationshipRow
{
    /// <summary>
    /// Relationship identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Child object identifier (see <see cref="Id"/>)
    /// </summary>
    public Id ChildId { get; set; }

    /// <summary>
    /// Relationship attributes (see <see cref="AttributeValue"/>)
    /// </summary>
    public List<AttributeValue> Attributes { get; set; } = new();

    /// <summary>
    /// File identifier (used in attachment relationships)
    /// </summary>
    public string FileId { get; set; }

    /// <summary>
    /// File name (used in attachment relationships)
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// File path (used in attachment relationships)
    /// </summary>
    public string FileResource => Path.Combine("data", FileId, FileName);
}
