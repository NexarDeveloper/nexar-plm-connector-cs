using System;
using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents file resource in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileResource
{
    /// <summary>
    /// Filename
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Data as bytearray string
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; set; }
}
