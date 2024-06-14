using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Query;

/// <summary>
/// Represents query in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class Query
{
    /// <summary>
    /// Object type name qualifier
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// List of custom query attributes (see <see cref="QueryAttribute"/>).
    /// </summary>
    public List<QueryAttribute> Attrs { get; set; } = new();

    /// <summary>
    /// Folder path qualifier
    /// </summary>
    public string FolderPath { get; set; }

    /// <summary>
    /// Object modification date qualifier. If present, query results should contains objects modified after this date.
    /// </summary>
    public long ModifyDate { get; set; }

    /// <summary>
    /// Maximum number of results expected to be returned from this query
    /// </summary>
    public long MaxRows { get; set; }
}
