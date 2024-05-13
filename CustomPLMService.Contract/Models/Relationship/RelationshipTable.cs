using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Relationship
{

    #region Relationships

    /// <summary>
    /// Represents object relationships table in external system. This table contains list of relationships of the object with other objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RelationshipTable
    {
        /// <summary>
        /// Relationship table identifier (see <see cref="Id"/>)
        /// </summary>
        public Id Id { get; set; }

        /// <summary>
        /// Relationship table type (see <see cref="RelationshipType"/>)
        /// </summary>
        public RelationshipType Type { get; set; }

        /// <summary>
        /// BOM redline change identifier, used when table represents BOM redline relationships (see <see cref="Id"/>)
        /// </summary>
        public Id RedLineChange { get; set; }

        /// <summary>
        /// Relationships contained by this table (see <see cref="RelationshipRow"/>)
        /// </summary>
        public List<RelationshipRow> Rows { get; set; } = new();
    }

    #endregion
}
