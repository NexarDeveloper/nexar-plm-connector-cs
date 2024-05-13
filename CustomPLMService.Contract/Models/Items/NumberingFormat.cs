using CustomPLMService.Contract.Models.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents object numbering format in external system. Numbering formats are defining how part (item) numbers are assigned to the new objects in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class NumberingFormat
{
    /// <summary>
    /// Numbering format identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Associated object type identifier (see <see cref="TypeId"/>)
    /// </summary>
    public TypeId TypeId { get; set; }

    /// <summary>
    /// Custom parameters map. Meaning depends on the implementation.
    /// </summary>
    public Dictionary<string, string> Fields { get; set; }
}
