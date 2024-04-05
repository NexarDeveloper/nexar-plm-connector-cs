using CustomPLMService.Contract.Models.Items;
namespace CustomPLMService.Contract.Models.Metadata;

/// <summary>
/// Represents attribute list value in external system.
/// </summary>
public class ListValue
{
    /// <summary>
    /// List value identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Actual value (see <see cref="Value"/>)
    /// </summary>
    public Value Value;
}
