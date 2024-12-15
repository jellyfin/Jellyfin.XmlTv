using System.Text;
using Jellyfin.XmlTv.Enums;

namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Class XmlTvReview.
/// </summary>
public class XmlTvReview
{
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public ReviewType Type { get; set; }

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the reviewer.
    /// </summary>
    public string? Reviewer { get; set; }

    /// <summary>
    /// Gets or sets the language.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the content.
    /// </summary>
    public string? Content { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("Type: ").Append(Type);
        if (!string.IsNullOrEmpty(Source))
        {
            builder.Append(", Source: ").Append(Source);
        }

        if (!string.IsNullOrEmpty(Reviewer))
        {
            builder.Append(", Reviewer: ").Append(Reviewer);
        }

        if (!string.IsNullOrEmpty(Language))
        {
            builder.Append(", Language: ").Append(Language);
        }

        if (!string.IsNullOrEmpty(Content))
        {
            builder.Append(", Content: ").Append(Content);
        }

        return builder.ToString();
    }
}
