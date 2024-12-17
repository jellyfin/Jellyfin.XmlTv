#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Jellyfin.XmlTv.Interfaces;

namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Program class.
/// </summary>
public class XmlTvProgram : IEquatable<XmlTvProgram>, IHasImages, IHasIcons, IHasUrls
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlTvProgram"/> class.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    public XmlTvProgram(string channelId)
    {
        ChannelId = channelId;
        Categories = [];
        Keywords = [];
        Countries = [];
        Episode = null;

        ProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        SeriesProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the id.
    /// </summary>
    public string ChannelId { get; }

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the program id.
    /// </summary>
    public string? ProgramId { get; set; }

    /// <summary>
    /// Gets or sets the quality.
    /// </summary>
    public string? Quality { get; set; }

    /// <summary>
    /// Gets or sets the original language.
    /// </summary>
    public string? OriginalLanguage { get; set; }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets the categories.
    /// </summary>
    public List<string> Categories { get; }

    /// <summary>
    /// Gets the keywords.
    /// </summary>
    public List<string> Keywords { get; }

    /// <summary>
    /// Gets the countries.
    /// </summary>
    public List<string> Countries { get; }

    /// <summary>
    /// Gets or sets the previously shown date.
    /// </summary>
    public DateTimeOffset? PreviouslyShown { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this program was previously shown.
    /// </summary>
    public bool IsPreviouslyShown { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this program is new.
    /// </summary>
    public bool IsNew { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this program is liven.
    /// </summary>
    public bool IsLive { get; set; }

    /// <summary>
    /// Gets or sets the copyright date.
    /// </summary>
    public DateTimeOffset? CopyrightDate { get; set; }

    /// <summary>
    /// Gets or sets the episode.
    /// </summary>
    public XmlTvEpisode? Episode { get; set; }

    /// <summary>
    /// Gets or sets the credits.
    /// </summary>
    public List<XmlTvCredit>? Credits { get; set; }

    /// <summary>
    /// Gets or sets the rating.
    /// </summary>
    public XmlTvRating? Rating { get; set; }

    /// <summary>
    /// Gets or sets the star rating.
    /// </summary>
    public float? StarRating { get; set; }

    /// <summary>
    /// Gets or sets the images.
    /// </summary>
    public List<XmlTvImage>? Images { get; set; }

    /// <summary>
    /// Gets or sets the premier information.
    /// </summary>
    public XmlTvPremiere? Premiere { get; set; }

    /// <summary>
    /// Gets the provider ids.
    /// </summary>
    public Dictionary<string, string> ProviderIds { get; }

    /// <summary>
    /// Gets the series provider ids.
    /// </summary>
    public Dictionary<string, string> SeriesProviderIds { get; }

    /// <summary>
    /// Gets or sets the icons.
    /// </summary>
    public List<XmlTvIcon>? Icons { get; set; }

    /// <summary>
    /// Gets or sets the URLs.
    /// </summary>
    public List<XmlTvUrl>? Urls { get; set; }

    /// <inheritdoc />
    public bool Equals(XmlTvProgram? other)
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
        return ChannelId == other.ChannelId
            && StartDate == other.StartDate
            && EndDate == other.EndDate;
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => (ChannelId.GetHashCode(StringComparison.Ordinal) * 17) + (StartDate.GetHashCode() * 17) + (EndDate.GetHashCode() * 17);

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendFormat(CultureInfo.InvariantCulture, "ChannelId: \t\t{0}\r\n", ChannelId);
        builder.AppendFormat(CultureInfo.InvariantCulture, "Title: \t\t{0}\r\n", Title);
        builder.AppendFormat(CultureInfo.InvariantCulture, "StartDate: \t\t{0}\r\n", StartDate);
        builder.AppendFormat(CultureInfo.InvariantCulture, "EndDate: \t\t{0}\r\n", EndDate);
        return builder.ToString();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is XmlTvProgram tvProgram && Equals(tvProgram);
}
