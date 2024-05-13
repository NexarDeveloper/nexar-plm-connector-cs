using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents attribute value in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class AttributeValue
{
    /// <summary>
    /// Attribute identifier
    /// </summary>
    public string AttributeId { get; set; }

    /// <summary>
    /// Actual attribute value (see <see cref="Value"/>)
    /// </summary>
    public Value Value { get; set; }
}
