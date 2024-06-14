using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents response from item modification call.
/// </summary>
[ExcludeFromCodeCoverage]
public class ItemResult
{
    public Item Item { get; set; }
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
}
