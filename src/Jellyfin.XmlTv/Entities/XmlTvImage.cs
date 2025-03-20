using System.Text;
using Jellyfin.XmlTv.Enums;

namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Image class.
/// </summary>
public class XmlTvImage
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public ImageType? Type { get; set; }

    /// <summary>
    /// Gets or sets the size.
    /// </summary>
    public ImageSize? Size { get; set; }

    /// <summary>
    /// Gets or sets the orientation.
    /// </summary>
    public ImageOrientation? Orientation { get; set; }

    /// <summary>
    /// Gets or sets the system.
    /// </summary>
    public string? System { get; set; }

    /// <summary>
    /// Gets or sets the path.
    /// </summary>
    public string? Path { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("Type: ").Append(Type);
        if (Size.HasValue)
        {
            builder.Append(", Size: ").Append(Size);
        }

        if (Orientation.HasValue)
        {
            builder.Append(", Orientation: ").Append(Orientation);
        }

        if (!string.IsNullOrEmpty(System))
        {
            builder.Append(", System: ").Append(System);
        }

        if (!string.IsNullOrEmpty(Path))
        {
            builder.Append(", Path: ").Append(Path);
        }

        return builder.ToString();
    }
}
