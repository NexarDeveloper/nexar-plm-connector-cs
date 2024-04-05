using CustomPLMService.Contract.Models.Metadata;
namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents object identifier in external system.
/// </summary>
public class Id
{
    /// <summary>
    /// The number of an item
    /// </summary>
    public string PublicId { get; set; }

    /// <summary>
    /// The unique internal identifier for an object
    /// </summary>
    public string PrivateId { get; set; }

    /// <summary>
    /// Object type identifier (see <see cref="TypeId"/>)
    /// </summary>
    public TypeId TypeId { get; set; }

    /// <summary>
    /// Checks whether specified <see cref="Id"/> instance can be considered equal to this object.
    /// </summary>
    /// <param name="other">Instance to compare</param>
    /// <returns></returns>
    protected bool Equals(Id other)
    {
        return string.Equals(PublicId, other.PublicId) && string.Equals(PrivateId, other.PrivateId) &&
            Equals(TypeId, other.TypeId);
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
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Id)obj);
    }

    /// <summary>
    /// Get object hashcode.
    /// </summary>
    /// <returns>Hashcode value</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (PublicId != null ? PublicId.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (PrivateId != null ? PrivateId.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (TypeId != null ? TypeId.GetHashCode() : 0);
            return hashCode;
        }
    }
}
