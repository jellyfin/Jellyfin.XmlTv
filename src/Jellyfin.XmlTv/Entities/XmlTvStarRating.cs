#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Jellyfin.XmlTv.Interfaces;

namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// StarRating class.
/// </summary>
public class XmlTvStarRating : IHasIcons
{
    /// <summary>
    /// Gets or sets the system.
    /// </summary>
    /// Example: TV Guide
    public string? System { get; set; }

    /// <summary>
    /// Gets or setsthe star rating.
    /// </summary>
    public decimal? StarRating { get; set; }

    /// <summary>
    /// Gets or sets the icons.
    /// </summary>
    public List<XmlTvIcon>? Icons { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder(StarRating?.ToString(CultureInfo.InvariantCulture));

        if (!string.IsNullOrEmpty(System))
        {
            builder.Append(" (").Append(System).Append(')');
        }

        return builder.ToString();
    }
}
