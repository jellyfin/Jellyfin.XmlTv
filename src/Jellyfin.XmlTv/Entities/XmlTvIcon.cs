using System.Text;

namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Class XmlTvIcon.
/// </summary>
public class XmlTvIcon
{
    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public int? Height { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("Source: ").Append(Source);
        if (Width.HasValue)
        {
            builder.Append(", Width: ").Append(Width);
        }

        if (Height.HasValue)
        {
            builder.Append(", Height: ").Append(Height);
        }

        return builder.ToString();
    }
}
