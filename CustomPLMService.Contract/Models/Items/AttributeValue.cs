namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents attribute value in external system.
/// </summary>
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
