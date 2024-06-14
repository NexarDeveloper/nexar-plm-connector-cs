using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Query;

/// <summary>
/// Represents query attribute in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class QueryAttribute
{
    /// <summary>
    /// Specifies how clauses are to occur in matching items.
    /// </summary>
    public enum Occurrences
    {
        /// <summary>
        /// Use this operator for clauses that should appear in the matching item.
        /// </summary>
        Should = 0,
        /// <summary>
        /// Use this operator for clauses that must appear in the matching items.
        /// </summary>
        Must = 1
    }

    /// <summary>
    /// Attribute name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Attribute value
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Attribute occurrence (see <see cref="Occurrences"/>)
    /// </summary>
    public Occurrences Occurrence { get; set; }
}
