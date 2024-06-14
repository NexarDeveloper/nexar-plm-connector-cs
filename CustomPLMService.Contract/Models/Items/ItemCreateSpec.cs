using CustomPLMService.Contract.Models.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents item creation specification. This information is used when creating new objects in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class ItemCreateSpec
{
    /// <summary>
    /// Numbering format information (see <see cref="NumberingFormat"/>).
    /// </summary>
    public NumberingFormat Autonumber { get; set; }

    /// <summary>
    /// Object type information (see <see cref="Type"/>)
    /// </summary>
    public Type Metadata { get; set; }

    /// <summary>
    /// List of attribute values for the new object (see <see cref="AttributeValue"/>)
    /// </summary>
    public List<AttributeValue> Values { get; set; } = new List<AttributeValue>();

    /// <summary>
    /// Requested object identifier for the new object (see <see cref="Id"/>)
    /// </summary>
    public Id SpecificId { get; set; }
}
