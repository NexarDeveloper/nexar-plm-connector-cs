using CustomPLMService.Contract.Models;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using CustomPLMService.Contract.Models.Relationship;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomPLMService.Contract.Models.Query;

namespace CustomPLMService.Contract
{
    /// <summary>
    /// Interface for operations on an external system.<br />
    /// Supported operations:
    /// <list type="bullet">
    /// <item>
    /// <term>TestAccess</term>
    /// <description> checks connection with an external system</description>
    /// </item>
    /// <item>
    /// <term>CreateItem</term>
    /// <description> creates an item in an external system based on the specification</description>
    /// </item>
    /// <item>
    /// <term>ReadItem</term>
    /// <description> reads an item with the specified identifier from an external system</description>
    /// </item>
    /// <item>
    /// <term>UpdateItem</term>
    /// <description> updates an item in an external system based on the specification</description>
    /// </item>
    /// <item>
    /// <term>DeleteItem</term>
    /// <description> deletes an item with specified id from an external system</description>
    /// </item>
    /// <item>
    /// <term>QueryItems</term>
    /// <description> finds items identifiers which meet specified query criteria</description>
    /// </item>
    /// <item>
    /// <term>CreateRelationships</term>
    /// <description> creates relationships between items</description>
    /// </item>
    /// <item>
    /// <term>ReadRelationship</term>
    /// <description> reads relationships between items with specified table identifier and type</description>
    /// </item>
    /// <item>
    /// <term>AdvanceState</term>
    /// <description> increases item's lifecycle state to the next default state</description>
    /// </item>
    /// <item>
    /// <term>IsOperationSupported</term>
    /// <description> checks if the external system supports given operation</description>
    /// </item>
    /// </list>
    /// </summary>
    public interface ICustomPlmService
    {
        /// <summary>
        /// Checks connection with an external system
        /// </summary>
        /// <param name="auth">Authentication data used to connect to the external system</param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if using authentication data results in a successful connection, false otherwise</returns>
        Task<bool> TestAccess(Auth auth, CancellationToken cancellationToken);

        /// <summary>
        /// <para>Creates items in an external system based on <paramref name="items"/></para>
        /// <para>If a specified identifier is not used, it will be generated</para>
        /// </summary>
        /// <param name="items">Items creation specification</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of created items</returns>
        /// <seealso cref="BaseType"/>
        Task<IEnumerable<Item>> CreateItems(IEnumerable<ItemCreateSpec> items, CancellationToken cancellationToken);

        /// <summary>
        /// Reads items with specified <paramref name="plmIds"/> from an external system
        /// </summary>
        /// <param name="plmIds">Item identifiers</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of found items if it exists, otherwise null</returns>
        Task<IEnumerable<Item>> ReadItems(IEnumerable<Id> plmIds, CancellationToken cancellationToken);

        /// <summary>
        /// Updates items in an external system based on <paramref name="updateSpecs"/>
        /// </summary>
        /// <param name="updateSpecs">List of item update specifications</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of updated items</returns>
        /// <seealso cref="BaseType"/>
        Task<IEnumerable<Item>> UpdateItems(IEnumerable<ItemUpdateSpec> updateSpecs, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes from an external system items with specified <paramref name="ids"/>
        /// </summary>
        /// <param name="ids">List of item identifier</param>
        /// <param name="cancellationToken"></param>
        Task DeleteItems(IEnumerable<Id> ids, CancellationToken cancellationToken);

        /// <summary>
        /// Finds items identifiers with <paramref name="type"/> which meet specified <paramref name="query"/>
        /// </summary>
        /// <param name="query">Query criteria to be meet</param>
        /// <param name="type">Item type</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of found items identifiers</returns>
        Task<IEnumerable<Id>> QueryItems(Query query, Type type, CancellationToken cancellationToken);

        /// <summary>
        /// Creates relationships between items from <paramref name="tables"/>
        /// </summary>
        /// <param name="tables">List of relationship tables to be created</param>
        /// <param name="cancellationToken"></param>
        /// <seealso cref="RelationshipType"/>
        Task CreateRelationships(IEnumerable<RelationshipTable> tables, CancellationToken cancellationToken);

        /// <summary>
        /// Reads relationships between items with the specified table <paramref name="ids"/> and <paramref name="type"/>
        /// </summary>
        /// <param name="ids">List of relationship table identifiers</param>
        /// <param name="type">Relationship's type</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Table with found relationships</returns>
        Task<IEnumerable<RelationshipTable>> ReadRelationships(IEnumerable<Id> ids, RelationshipType type, CancellationToken cancellationToken);

        /// <summary>
        /// Increases item's lifecycle state to the next default state
        /// </summary>
        /// <param name="id">Item identifier</param>
        /// <param name="cancellationToken"></param>
        Task AdvanceState(Id id, CancellationToken cancellationToken);

        /// <summary>
        /// Checks if an external system supports given <paramref name="operationType"/>
        /// </summary>
        /// <param name="operationType">Operation type to be checked</param>
        /// <param name="cancellationToken"></param>
        /// <returns>True if an external system supports given operation, false otherwise</returns>
        Task<bool> IsOperationSupported(SupportedOperation operationType, CancellationToken cancellationToken);

        /// <summary>
        /// Stores a file in an external system
        /// </summary>
        /// <param name="request">File name and content as byte array</param>
        /// <param name="cancellationToken"></param>
        Task<string> UploadFile(FileResource request, CancellationToken cancellationToken);
    }
}
