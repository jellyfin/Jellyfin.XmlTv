using System.Collections.Generic;
using System.Globalization;
using Jellyfin.XmlTv.Enums;

namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Credit class.
/// </summary>
public class XmlTvCredit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlTvCredit"/> class.
    /// </summary>
    /// <param name="type">The <see cref="CreditType"/>.</param>
    /// <param name="guest">a value indicating whether the credit is a guest.</param>
    public XmlTvCredit(CreditType type, bool guest = false)
    {
        Type = type;
        Guest = guest;
    }

    /// <summary>
    /// Gets the type.
    /// </summary>
    public CreditType Type { get; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the credit is a guest.
    /// </summary>
    public bool Guest { get; set; }

    /// <summary>
    /// Gets or sets the images.
    /// </summary>
    public IReadOnlyList<XmlTvImage>? Images { get; set; }

    /// <summary>
    /// Gets or sets the urls.
    /// </summary>
    public IReadOnlyList<XmlTvUrl>? Urls { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => string.Format(
            CultureInfo.InvariantCulture,
            "{0} - ({1})",
            Name,
            Type);
}
