using CustomPLMService.Contract.Models.Metadata;
namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents value in external system.
/// </summary>
public class Value
{
    private readonly object typedValue;

    /// <summary>
    /// Enumerates possible value types.
    /// </summary>
    public enum Type
    {
        /// <summary>
        /// Undefined value type
        /// </summary>
        None,
        /// <summary>
        /// String value type
        /// </summary>
        String,
        /// <summary>
        /// Boolean value type
        /// </summary>
        Bool,
        /// <summary>
        /// Double numeric value type
        /// </summary>
        Double,
        /// <summary>
        /// Float numeric value type
        /// </summary>
        Float,
        /// <summary>
        /// Integer numeric value type
        /// </summary>
        Int,
        /// <summary>
        /// Date/time value type
        /// </summary>
        Date,
        /// <summary>
        /// Object reference value type
        /// </summary>
        Reference,
        /// <summary>
        /// Unit of Measure value type
        /// </summary>
        Uom,
        /// <summary>
        /// List value type
        /// </summary>
        ListValue
    }

    /// <summary>
    /// Value type (see <see cref="Type"/>)
    /// </summary>
    public Type TypedValueCase { get; }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with null value and undefined type.
    /// </summary>
    public Value()
    {
        TypedValueCase = Type.None;
        typedValue = null;
    }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with type <see cref="Type.String"/>.
    /// </summary>
    /// <param name="value">String value</param>
    public Value(string value)
    {
        TypedValueCase = Type.String;
        typedValue = value;
    }

    /// <summary>
    /// Converts specified value to string.
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>
    /// String value or null, if input is null or has internal null value
    /// </returns>
    public static implicit operator string(Value value)
    {
        return (string)value?.typedValue;
    }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with type <see cref="Type.Bool"/>.
    /// </summary>
    /// <param name="value">Boolean value</param>
    public Value(bool value)
    {
        TypedValueCase = Type.Bool;
        typedValue = value;
    }

    /// <summary>
    /// Converts specified value to boolean.
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>
    /// Boolean value or false, if input is null or has internal null value
    /// </returns>
    public static implicit operator bool(Value value)
    {
        return (bool)(value?.typedValue ?? false);
    }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with type <see cref="Type.Double"/>.
    /// </summary>
    /// <param name="value">Double numeric value</param>
    public Value(double value)
    {
        TypedValueCase = Type.Double;
        typedValue = value;
    }

    /// <summary>
    /// Converts specified value to double.
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>
    /// Double value or 0, if input is null or has internal null value
    /// </returns>
    public static implicit operator double(Value value)
    {
        return (double)(value?.typedValue ?? 0d);
    }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with type <see cref="Type.Float"/>.
    /// </summary>
    /// <param name="value">Float numeric value</param>
    public Value(float value)
    {
        TypedValueCase = Type.Float;
        typedValue = value;
    }

    /// <summary>
    /// Converts specified value to float.
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>
    /// Float value or 0, if input is null or has internal null value
    /// </returns>
    public static implicit operator float(Value value)
    {
        return (float)(value?.typedValue ?? 0f);
    }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with type <see cref="Type.Int"/>.
    /// </summary>
    /// <param name="value">Integer numeric value</param>
    public Value(int value)
    {
        TypedValueCase = Type.Int;
        typedValue = value;
    }

    /// <summary>
    /// Converts specified value to integer.
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>
    /// Integer value or 0, if input is null or has internal null value
    /// </returns>
    public static implicit operator int(Value value)
    {
        return (int)(value?.typedValue ?? 0);
    }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with type <see cref="Type.Date"/>.
    /// </summary>
    /// <param name="value">Long numeric value (timestamp)</param>
    public Value(long value)
    {
        TypedValueCase = Type.Date;
        typedValue = value;
    }

    /// <summary>
    /// Converts specified value to long.
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>
    /// Long value or 0, if input is null or has internal null value
    /// </returns>
    public static implicit operator long(Value value)
    {
        return (long)(value?.typedValue ?? 0);
    }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with type <see cref="Type.Reference"/>.
    /// </summary>
    /// <param name="value">Object reference</param>
    public Value(Id value)
    {
        TypedValueCase = Type.Reference;
        typedValue = value;
    }

    /// <summary>
    /// Whether internal value is null.
    /// </summary>
    /// <returns>True if internal value is null, false otherwise</returns>
    public bool IsNull()
    {
        return typedValue is null;
    }

    /// <summary>
    /// Converts specified value to object reference (see <see cref="Id"/>).
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>
    /// Object reference or null, if input is null or has internal null value
    /// </returns>
    public static implicit operator Id(Value value)
    {
        return (Id)value?.typedValue;
    }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with type <see cref="Type.Uom"/>.
    /// </summary>
    /// <param name="value">Unit of Measure value</param>
    public Value(UomValue value)
    {
        TypedValueCase = Type.Uom;
        typedValue = value;
    }

    /// <summary>
    /// Converts specified value to Unit of Measure value (see <see cref="UomValue"/>).
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>
    /// Unit of Measure value or null, if input is null or has internal null value
    /// </returns>
    public static implicit operator UomValue(Value value)
    {
        return (UomValue)value?.typedValue;
    }

    /// <summary>
    /// Creates instance of <see cref="Value"/> with type <see cref="Type.ListValue"/>.
    /// </summary>
    /// <param name="value">List value</param>
    public Value(ListValue value)
    {
        TypedValueCase = Type.ListValue;
        typedValue = value;
    }

    /// <summary>
    /// Converts specified value to list value (see <see cref="ListValue"/>).
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>
    /// List value or null, if input is null or has internal null value
    /// </returns>
    public static implicit operator ListValue(Value value)
    {
        return (ListValue)value?.typedValue;
    }
}
