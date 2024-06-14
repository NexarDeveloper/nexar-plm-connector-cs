using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Metadata;

/// <summary>
/// Represents object type identifier in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class TypeId
{
    /// <summary>
    /// Object type name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Object type internal identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Object type API name
    /// </summary>
    public string ApiName { get; set; }

    /// <summary>
    /// Base object type (see <see cref="BaseType"/>)
    /// </summary>
    public BaseType BaseType { get; set; }

    /// <summary>
    /// Checks whether specified object can be considered equal to this object.
    /// </summary>
    /// <param name="other">Object to compare</param>
    /// <returns>True if objects can be considered equal, false otherwise</returns>
    protected bool Equals(TypeId other)
    {
        return string.Equals(Name, other.Name) && string.Equals(Id, other.Id) &&
            string.Equals(ApiName, other.ApiName) && BaseType == other.BaseType;
    }

    /// <summary>
    /// Checks whether specified object can be considered equal to this object.
    /// </summary>
    /// <param name="obj">Object to compare</param>
    /// <returns>True if objects can be considered equal, false otherwise</returns>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TypeId)obj);
    }

    /// <summary>
    /// Get object hashcode.
    /// </summary>
    /// <returns>Hashcode value</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (Name != null ? Name.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ApiName != null ? ApiName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int)BaseType;
            return hashCode;
        }
    }

    /// <summary>
    /// Provides string representation of the object.
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        return
            $"{nameof(Name)}: {Name}, {nameof(Id)}: {Id}, {nameof(ApiName)}: {ApiName}, {nameof(BaseType)}: {BaseType}";
    }
}
