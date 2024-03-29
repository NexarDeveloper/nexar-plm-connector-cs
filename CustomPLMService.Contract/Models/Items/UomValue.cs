namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents Unit of Measure value.
/// </summary>
public class UomValue
{
    /// <summary>
    /// Unit of measure name
    /// </summary>
    public string UnitName { get; set; }

    /// <summary>
    /// Numeric value
    /// </summary>
    public double UnitValue { get; set; }
}
