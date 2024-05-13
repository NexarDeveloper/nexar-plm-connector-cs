using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Metadata;

/// <summary>
/// Represents attribute specification in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class AttributeSpec
{
    /// <summary>
    /// Enumerates possible attribute data types.
    /// </summary>
    public enum Datatype
    {
        /// <summary>
        /// String type
        /// </summary>
        Text = 0,
        /// <summary>
        /// Numeric type
        /// </summary>
        Number = 1,
        /// <summary>
        /// Date type
        /// </summary>
        Date = 2,
        /// <summary>
        /// Unit of measure type
        /// </summary>
        Uom = 3,
        /// <summary>
        /// Object type
        /// </summary>
        Object = 4,
        /// <summary>
        /// Boolean type
        /// </summary>
        Boolean = 5
    }

    /// <summary>
    /// Enumerates possible restrictions for attribute values.
    /// </summary>
    public enum Valueset
    {
        /// <summary>
        /// Any value is acceptable
        /// </summary>
        Free = 0,
        /// <summary>
        /// Value must be on the predefined list of values for a given attribute
        /// </summary>
        List = 1,
        /// <summary>
        /// Any value is acceptable, however attribute specification does provide a set of predefined values
        /// </summary>
        Hybrid = 2
    }

    private string id;

    /// <summary>
    /// Attribute identifier
    /// </summary>
    public string Id
    {
        get
        {
            if (id == null)
            {
                return ApiName;
            }

            return id;
        }

        set => id = value;
    }

    /// <summary>
    /// Attribute API name
    /// </summary>
    public string ApiName { get; set; }

    /// <summary>
    /// Attribute display name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Attribute category
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Whether attribute can have multiple values
    /// </summary>
    public bool MultiValued { get; set; }

    /// <summary>
    /// Whether attribute is read-only
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Whether attribute is required
    /// </summary>
    /// <remarks>Required attributes must be set during object creation in external system.</remarks>
    public bool Required { get; set; }

    /// <summary>
    /// Whether attribute is built-in
    /// </summary>
    public bool BuiltIn { get; set; }

    /// <summary>
    /// Attribute unit of measure family name (applicable for Unit of Measure attributes only).
    /// </summary>
    public string UomFamilyName { get; set; }

    /// <summary>
    /// Attribute data type (see <see cref="Datatype"/>).
    /// </summary>
    public Datatype DataType { get; set; }

    /// <summary>
    /// Attribute value restriction type (see <see cref="ValuesetType"/>)
    /// </summary>
    public Valueset ValuesetType { get; set; }

    /// <summary>
    /// List of possible values for the attribute. Applicable for attributes which represent non-object list values.
    /// </summary>
    /// <seealso cref="ListValue"/>
    public List<ListValue> ListValues { get; set; } = new List<ListValue>();

    /// <summary>
    /// Provides string representation of the object.
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        return
            $"{nameof(Id)}: {Id}, {nameof(ApiName)}: {ApiName}, {nameof(Name)}: {Name}, {nameof(Category)}: {Category}, {nameof(MultiValued)}: {MultiValued}, {nameof(ReadOnly)}: {ReadOnly}, {nameof(Required)}: {Required}, {nameof(BuiltIn)}: {BuiltIn}, {nameof(UomFamilyName)}: {UomFamilyName}, {nameof(DataType)}: {DataType}, {nameof(ValuesetType)}: {ValuesetType}, {nameof(ListValues)}: {ListValues}";
    }
}
