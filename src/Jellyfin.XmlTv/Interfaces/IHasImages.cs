#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only

using System.Collections.Generic;
using Jellyfin.XmlTv.Entities;

namespace Jellyfin.XmlTv.Interfaces;

/// <summary>
/// An abstraction representing an entity that has images.
/// </summary>
public interface IHasImages
{
    /// <summary>
    /// Gets or sets a collection containing this entity's images.
    /// </summary>
    List<XmlTvImage>? Images { get; set; }
}
