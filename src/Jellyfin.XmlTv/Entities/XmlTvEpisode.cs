using System.Text;

namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Episode class.
/// </summary>
public class XmlTvEpisode
{
    /// <summary>
    /// Gets or sets the series id.
    /// </summary>
    public int? Series { get; set; }

    /// <summary>
    /// Gets or sets the series count.
    /// </summary>
    public int? SeriesCount { get; set; }

    /// <summary>
    /// Gets or sets the episode number.
    /// </summary>
    public int? Episode { get; set; }

    /// <summary>
    /// Gets or sets the episode count.
    /// </summary>
    public int? EpisodeCount { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the part.
    /// </summary>
    public int? Part { get; set; }

    /// <summary>
    /// Gets or sets the part count.
    /// </summary>
    public int? PartCount { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        if (Series.HasValue || SeriesCount.HasValue)
        {
            builder.Append("Series ");
            if (Series.HasValue)
            {
                builder.Append(Series);
            }
            else
            {
                builder.Append('?');
            }

            if (SeriesCount.HasValue)
            {
                builder.Append(" of ").Append(SeriesCount);
            }
        }

        if (Episode.HasValue || EpisodeCount.HasValue)
        {
            builder.Append(builder.Length > 0 ? ", " : string.Empty);
            builder.Append("Episode ");

            if (Episode.HasValue)
            {
                builder.Append(Episode);
            }
            else
            {
                builder.Append('?');
            }

            if (EpisodeCount.HasValue)
            {
                builder.Append(" of ").Append(EpisodeCount);
            }
        }

        if (Part.HasValue || PartCount.HasValue)
        {
            builder.Append(builder.Length > 0 ? ", " : string.Empty);
            builder.Append("Part ");
            if (Part.HasValue)
            {
                builder.Append(Part);
            }
            else
            {
                builder.Append('?');
            }

            if (PartCount.HasValue)
            {
                builder.Append(" of ").Append(PartCount);
            }
        }

        return builder.ToString();
    }
}
