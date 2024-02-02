using System.Collections.Generic;
using System.IO;

namespace CustomPLMService.Contract
{
    #region Authentication

    /// <summary>
    /// Represents credentials in external system.
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// Account name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Account password
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Provides context for user authentication.
    /// </summary>
    public class Auth
    {
        /// <summary>
        /// External system instance URL
        /// </summary>
        public string PlmUrl { get; set; }

        /// <summary>
        /// Nexus session identifier
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// External system account credentials (see <see cref="Credentials"/>)
        /// </summary>
        public Credentials Credentials { get; set; }

        public List<string> Licenses { get; set; } = new List<string>();

        public string Context { get; set; }
    }

    /// <summary>
    /// Provides context for authentication in external system.
    /// </summary>
    public class Context
    {
        private readonly string token;
        private readonly Credentials credentials;

        /// <summary>
        /// Creates instance of authentication context.
        /// </summary>
        /// <param name="credentials">Account credentials (<see cref="Credentials"/>)</param>
        public Context(Credentials credentials)
        {
            this.credentials = credentials;
        }

        public Context(string token)
        {
            this.token = token;
        }

        /// <summary>
        /// User name
        /// </summary>
        public string Username => credentials.Username;

        /// <summary>
        /// User password
        /// </summary>
        public string Password => credentials.Password;
        public string Token => token;
    }

    /// <summary>
    /// Enumerates supported special operations in external systems.
    /// </summary>
    public enum SupportedOperation
    {
        /// <summary>
        /// Create Change Order operation
        /// </summary>
        CreateChangeOrder = 0,
        /// <summary>
        /// Support for part choices creation based on item's attributes
        /// </summary>
        ExtractPartChoicesFromAttributes = 1
    }

    #endregion

    #region Metadata Model  

    /// <summary>
    /// Enumerates possible base object types in external system.
    /// </summary>
    public enum BaseType
    {
        /// <summary>
        /// Part (item) type
        /// </summary>
        Item = 0,
        
        /// <summary>
        /// Change Object type
        /// </summary>
        Change = 1
    }

    /// <summary>
    /// Represents object type identifier in external system.
    /// </summary>
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
            return Equals((TypeId) obj);
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
                hashCode = (hashCode * 397) ^ (int) BaseType;
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

    /// <summary>
    /// Represents attribute list value in external system.
    /// </summary>
    public class ListValue
    {
        /// <summary>
        /// List value identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Actual value (see <see cref="Value"/>)
        /// </summary>
        public Value Value;
    }

    /// <summary>
    /// Represents attribute specification in external system.
    /// </summary>
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

    /// <summary>
    /// Enumerates possible object relationship types in external system.
    /// </summary>
    public enum RelationshipType
    {
        /// <summary>
        /// BOM relationship type
        /// </summary>
        Bom = 0,

        /// <summary>
        /// File (attachments) relationship type
        /// </summary>
        Attachments = 1,

        /// <summary>
        /// Manufacturer part relationship type
        /// </summary>
        ManufacturerParts = 2,

        /// <summary>
        /// Affected items relationship type
        /// </summary>
        AffectedItems = 3
    }

    /// <summary>
    /// Represents relationship specification in external system.
    /// </summary>
    public class RelationshipSpec
    {
        /// <summary>
        /// Relationship type (see <see cref="RelationshipType"/>)
        /// </summary>
        public RelationshipType Type { get; set; }

        /// <summary>
        /// Relationship attribute specifications (see <see cref="AttributeSpec"/>)
        /// </summary>
        public List<AttributeSpec> Attributes { get; set; } = new List<AttributeSpec>();
    }

    /// <summary>
    /// Represents object type in external system.
    /// </summary>
    public class Type
    {
        /// <summary>
        /// Object type identifier (see <see cref="TypeId"/>)
        /// </summary>
        public TypeId Id { get; set; }

        /// <summary>
        /// Object type attribute specifications (see <see cref="AttributeSpec"/>)
        /// </summary>
        public List<AttributeSpec> Attributes { get; set; } = new List<AttributeSpec>();

        /// <summary>
        /// Object type relationship specifications (see <see cref="RelationshipSpec"/>)
        /// </summary>
        public List<RelationshipSpec> Relationships { get; set; } = new List<RelationshipSpec>();
    }

    #endregion

    #region Items

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
            return Equals((Id) obj);
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

    /// <summary>
    /// Represents object numbering format in external system. Numbering formats are defining how part (item) numbers are assigned to the new objects in external system.
    /// </summary>
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

    // TODO cleanup

    /// <summary>
    /// Represents item creation specification. This information is used when creating new objects in external system.
    /// </summary>
    public class ItemCreateSpec
    {
        /// <summary>
        /// Numbering format information (see <see cref="NumberingFormat"/>).
        /// </summary>
        public NumberingFormat Autonumber { get; set; }

        /// <summary>
        /// Object type information (see <see cref="Type"/>)
        /// </summary>
        public Type Metadata { get; set; }

        /// <summary>
        /// List of attribute values for the new object (see <see cref="AttributeValue"/>)
        /// </summary>
        public List<AttributeValue> Values { get; set; } = new List<AttributeValue>();

        /// <summary>
        /// Requested object identifier for the new object (see <see cref="Id"/>)
        /// </summary>
        public Id SpecificId { get; set; }
    }

    // TODO cleanup - do we need this specific message

    /// <summary>
    /// Represents item update specification. This information is used when updating existing objects in external system.
    /// </summary>
    public class ItemUpdateSpec
    {
        /// <summary>
        /// Existing object identifier (see <see cref="Id"/>)
        /// </summary>
        public Id Id { get; set; }

        /// <summary>
        /// Attribute values to be updated (see <see cref="AttributeValue"/>)
        /// </summary>
        public List<AttributeValue> Values { get; set; } = new List<AttributeValue>();

        /// <summary>
        /// Associated object type information (see <see cref="Type"/>)
        /// </summary>
        public Type Metadata { get; set; }
    }

    /// <summary>
    /// Represents object in external system.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Object identifier (see <see cref="Id"/>)
        /// </summary>
        public Id Id { get; set; }

        /// <summary>
        /// Object attribute values (see <see cref="AttributeValue"/>)
        /// </summary>
        public List<AttributeValue> Values { get; set; } = new List<AttributeValue>();

        /// <summary>
        /// If present, provides information needed to view object information in external system (for example, web page URL).
        /// </summary>
        public string Link { get; set; }
    }

    /// <summary>
    /// Represents Unit of Measure value.
    /// </summary>
    public class UomValue
    {
        /// <summary>
        /// Unit of measure name
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// Numeric value
        /// </summary>
        public double UnitValue { get; set; }
    }
    
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
        public Type TypedValueCase { get;}

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
            if (value == null || value.typedValue == null)
            {
                return null;
            }
            return (string) value.typedValue;
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
            if (value == null || value.typedValue == null)
            {
                return false;
            }
            return (bool) value.typedValue;
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
            if (value == null || value.typedValue == null)
            {
                return 0;
            }
            return (double) value.typedValue;
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
            if (value == null || value.typedValue == null)
            {
                return 0;
            }
            return (float) value.typedValue;
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
            if (value == null || value.typedValue == null)
            {
                return 0;
            }
            return (int) value.typedValue;
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
            if (value == null || value.typedValue == null)
            {
                return 0;
            }
            return (long) value.typedValue;
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
            return typedValue == null;
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
            if (value == null || value.typedValue == null)
            {
                return null;
            }
            return (Id) value.typedValue;
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
            if (value == null || value.typedValue == null)
            {
                return null;
            }
            return (UomValue) value.typedValue;
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
            if (value == null || value.typedValue == null)
            {
                return null;
            }
            return (ListValue) value.typedValue;
        }
    }

    /// <summary>
    /// Represents attribute value in external system.
    /// </summary>
    public class AttributeValue
    {
        /// <summary>
        /// Attribute identifier
        /// </summary>
        public string AttributeId { get; set; }

        /// <summary>
        /// Actual attribute value (see <see cref="Value"/>)
        /// </summary>
        public Value Value { get; set; }
    }

    #endregion

    #region Search


    /// <summary>
    /// Represents query attribute in external system.
    /// </summary>
    public class QueryAttribute
    {
        /// <summary>
        /// Specifies how clauses are to occur in matching items.
        /// </summary>
        public enum Occurrences
        {
            /// <summary>
            /// Use this operator for clauses that should appear in the matching item.
            /// </summary>
            Should = 0,
            /// <summary>
            /// Use this operator for clauses that must appear in the matching items.
            /// </summary>
            Must = 1
        }

        /// <summary>
        /// Attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Attribute value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Attribute occurrence (see <see cref="Occurrences"/>)
        /// </summary>
        public Occurrences Occurrence { get; set; }
    }

    /// <summary>
    /// Represents query in external system.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Object type name qualifier
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// List of custom query attributes (see <see cref="QueryAttribute"/>).
        /// </summary>
        public List<QueryAttribute> Attrs { get; set; } = new List<QueryAttribute>();

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

    #endregion

    #region Relationships

    /// <summary>
    /// Represents object relationship in external system. Object relationship connects a pair of objects (parent and child).
    /// There are different types of relationships (see <see cref="RelationshipType"/>). 
    /// </summary>
    public class RelationshipRow
    {
        /// <summary>
        /// Relationship identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Child object identifier (see <see cref="Id"/>)
        /// </summary>
        public Id ChildId { get; set; }

        /// <summary>
        /// Relationship attributes (see <see cref="AttributeValue"/>)
        /// </summary>
        public List<AttributeValue> Attributes { get; set; } = new List<AttributeValue>();

        /// <summary>
        /// File identifier (used in attachment relationships)
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// File name (used in attachment relationships)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File path (used in attachment relationships)
        /// </summary>
        public string FileResource => Path.Combine("data", FileId, FileName);
    }

    /// <summary>
    /// Represents object relationships table in external system. This table contains list of relationships of the object with other objects.
    /// </summary>
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
        public List<RelationshipRow> Rows { get; set; } = new List<RelationshipRow>();
    }

    #endregion
}
