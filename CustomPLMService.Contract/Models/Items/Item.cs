﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace CustomPLMService.Contract.Models.Items;

/// <summary>
/// Represents object in external system.
/// </summary>
[ExcludeFromCodeCoverage]
public class Item
{
    /// <summary>
    /// Object identifier (see <see cref="Id"/>)
    /// </summary>
    public Id Id { get; set; }

    /// <summary>
    /// Object attribute values (see <see cref="AttributeValue"/>)
    /// </summary>
    public List<AttributeValue> Values { get; set; } = [];

    /// <summary>
    /// If present, provides information needed to view object information in external system (for example, web page URL).
    /// </summary>
    public string Link { get; set; }
}
