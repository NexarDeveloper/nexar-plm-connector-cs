using CustomPLMService.Contract.Models.Metadata;
using System.Collections.Generic;
namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents item update specification. This information is used when updating existing objects in external system.
/// </summary>
public class ItemUpdateSpec
{
    /// <summary>
    /// Existing object identifier (see <see cref="Id"/>)
    /// </summary>
    public Id Id { get; set; }

    /// <summary>
    /// Attribute values to be updated (see <see cref="AttributeValue"/>)
    /// </summary>
    public List<AttributeValue> Values { get; set; } = new List<AttributeValue>();

    /// <summary>
    /// Associated object type information (see <see cref="Type"/>)
    /// </summary>
    public Type Metadata { get; set; }
}
