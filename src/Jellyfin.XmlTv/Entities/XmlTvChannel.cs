#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only

using System;
using System.Collections.Generic;
using System.Text;
using Jellyfin.XmlTv.Interfaces;

namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Channel class.
/// </summary>
public class XmlTvChannel : IEquatable<XmlTvChannel>, IHasIcons, IHasUrls
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlTvChannel"/> class.
    /// </summary>
    /// <param name="id">The id.</param>
    public XmlTvChannel(string id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the Id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the number.
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// Gets or sets the URLs.
    /// </summary>
    public List<XmlTvUrl>? Urls { get; set; }

    /// <summary>
    /// Gets or sets the icons.
    /// </summary>
    public List<XmlTvIcon>? Icons { get; set; }

    /// <inheritdoc />
    public bool Equals(XmlTvChannel? other)
    {
        // If both are null, or both are same instance, return true.
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // If the other is null then return false
        if (other is null)
        {
            return false;
        }

        // Return true if the fields match:
        return Id == other.Id;
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => Id.GetHashCode(StringComparison.Ordinal) * 17;

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(Id).Append(" - ").Append(DisplayName).Append(' ');

        return builder.ToString();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is XmlTvChannel channel && Equals(channel);
}
