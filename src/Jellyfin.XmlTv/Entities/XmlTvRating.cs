#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only

using System.Collections.Generic;
using System.Text;
using Jellyfin.XmlTv.Interfaces;

namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Describes the rating (certification) applied to a program.
/// </summary>
public class XmlTvRating : IHasIcons
{
    /// <summary>
    /// Gets or sets the literal name of the rating system.
    /// </summary>
    /// Example: MPAA
    public string? System { get; set; }

    /// <summary>
    /// Gets or sets the rating using the system specified.
    /// </summary>
    // Example: TV-14
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the icons.
    /// </summary>
    public List<XmlTvIcon>? Icons { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder(Value);
        if (!string.IsNullOrEmpty(System))
        {
            builder.Append(" (").Append(System).Append(')');
        }

        return builder.ToString();
    }
}
