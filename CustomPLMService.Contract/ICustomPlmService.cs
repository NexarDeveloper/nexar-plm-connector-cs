using CustomPLMService.Contract.Models;
using CustomPLMService.Contract.Models.Authentication;
using CustomPLMService.Contract.Models.Items;
using CustomPLMService.Contract.Models.Metadata;
using CustomPLMService.Contract.Models.Relationship;
using CustomPLMService.Contract.Models.Search;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// <returns>True if using authentication data results in a successful connection, false otherwise</returns>
        Task<bool> TestAccess(Auth auth);

        /// <summary>
        /// <para>Creates an item in an external system based on <paramref name="item"/></para>
        /// <para>If a specified identifier is not used, it will be generated</para>
        /// </summary>
        /// <param name="context">Immutable operation context</param>
        /// <param name="item">Item creation specification</param>
        /// <returns>Created item</returns>
        /// <seealso cref="BaseType"/>
        Task<Item> CreateItem(ItemCreateSpec item);
        /// <summary>
        /// Reads an item with specified <paramref name="plmId"/> from an external system
        /// </summary>
        /// <param name="context">Immutable operation context</param>
        /// <param name="plmId">Item identifier</param>
        /// <returns>Found item if it exists, otherwise null</returns>
        Task<Item> ReadItem(Id plmId);
        /// <summary>
        /// Updates an item in an external system based on <paramref name="updateSpec"/>
        /// </summary>
        /// <param name="context">Immutable operation context</param>
        /// <param name="updateSpec">Item update specification</param>
        /// <returns>Updated item</returns>
        /// <seealso cref="BaseType"/>
        Task<Item> UpdateItem(ItemUpdateSpec updateSpec);
        /// <summary>
        /// Deletes from an external system an item with specified <paramref name="id"/>
        /// </summary>
        /// <param name="context">Immutable operation context</param>
        /// <param name="id">Item identifier</param>
        Task DeleteItem(Id id);
        /// <summary>
        /// Finds items identifiers with <paramref name="type"/> which meet specified <paramref name="query"/>
        /// </summary>
        /// <param name="context">Immutable operation context</param>
        /// <param name="query">Query criteria to be meet</param>
        /// <param name="type">Item type</param>
        /// <returns>List of found items identifiers</returns>
        Task<IEnumerable<Id>> QueryItems(Query query, Type type);
        /// <summary>
        /// Creates relationships between items from <paramref name="tables"/>
        /// </summary>
        /// <param name="context">Immutable operation context</param>
        /// <param name="tables">List of relationship tables to be created</param>
        /// <seealso cref="RelationshipType"/>
        Task CreateRelationships(IEnumerable<RelationshipTable> tables);
        /// <summary>
        /// Reads relationships between items with the specified table <paramref name="id"/> and <paramref name="type"/>
        /// </summary>
        /// <param name="context">Immutable operation context</param>
        /// <param name="id">Relationship table identifier</param>
        /// <param name="type">Relationship's type</param>
        /// <returns>Table with found relationships</returns>
        Task<RelationshipTable> ReadRelationship(Id id, RelationshipType type);
        /// <summary>
        /// Increases item's lifecycle state to the next default state
        /// </summary>
        /// <param name="context">Immutable operation context</param>
        /// <param name="id">Item identifier</param>
        Task AdvanceState(Id id);
        /// <summary>
        /// Checks if an external system supports given <paramref name="operationType"/>
        /// </summary>
        /// <param name="operationType">Operation type to be checked</param>
        /// <returns>True if an external system supports given operation, false otherwise</returns>
        Task<bool> IsOperationSupported(SupportedOperation operationType);

        /// <summary>
        /// Stores a file in an external system
        /// </summary>
        /// <param name="request">File name and content as byte array</param>
        Task<string> UploadFile(FileResource request);
    }
}
