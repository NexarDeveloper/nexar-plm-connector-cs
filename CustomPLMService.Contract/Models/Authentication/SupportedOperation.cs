namespace CustomPLMService.Contract.Models.Authentication;

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
    ExtractPartChoicesFromAttributes = 1,
    IncrementalPartChoicesSync = 2,
    AdvanceChangeOrder = 3,
    CreateInfoNumbering = 4,
    PublishWithNoBomSectionInConfig = 5,
    CreateMfrParts = 6,
}
