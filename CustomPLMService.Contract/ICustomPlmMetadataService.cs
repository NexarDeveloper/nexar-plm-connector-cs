using CustomPLMService.Contract.Models.Metadata;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace CustomPLMService.Contract
{
    /// <summary>
    /// Interface for retrieving metadata from an external system.<br />
    /// Supported operations:
    /// <list type="bullet">
    /// <item>
    /// <term>Read type identifiers</term>
    /// <description> Reads available type identifiers for a given base type from an external system</description>
    /// </item>
    /// <item>
    /// <term>Read types</term>
    /// <description> Reads types for a given list of type identifiers from an external system</description>
    /// </item>
    /// </list>
    /// </summary>
    public interface ICustomPlmMetadataService
    {
        /// <summary>
        /// Reads available type identifiers for the given <paramref name="baseType"/> from an external system
        /// </summary>
        /// <param name="baseType">Base object type</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of available type identifiers</returns>
        Task<IEnumerable<TypeId>> ReadTypeIdentifiers(BaseType baseType, CancellationToken cancellationToken);
        /// <summary>
        /// Reads types for the given <paramref name="typeId"/> from an external system
        /// </summary>
        /// <param name="typeId">List of type identifiers</param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of types</returns>
        Task<IEnumerable<Type>> ReadTypes(IEnumerable<TypeId> typeId, CancellationToken cancellationToken);
    }
}
