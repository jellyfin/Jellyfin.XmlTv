#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only

using System.Collections.Generic;
using Jellyfin.XmlTv.Entities;

namespace Jellyfin.XmlTv.Interfaces;

/// <summary>
/// An abstraction representing an entity that has icons.
/// </summary>
public interface IHasIcons
{
    /// <summary>
    /// Gets or sets a collection containing this entity's icons.
    /// </summary>
    public List<XmlTvIcon>? Icons { get; set; }
}
