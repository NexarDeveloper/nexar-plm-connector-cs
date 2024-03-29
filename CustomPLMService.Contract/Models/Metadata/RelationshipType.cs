namespace CustomPLMService.Contract.Models.Metadata;

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
