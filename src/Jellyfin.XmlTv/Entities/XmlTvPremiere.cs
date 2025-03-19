namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Premier class.
/// </summary>
public class XmlTvPremiere
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlTvPremiere"/> class.
    /// </summary>
    /// <param name="details">The details.</param>
    public XmlTvPremiere(string details)
    {
        Details = details;
    }

    /// <summary>
    /// Gets the details denoting the first showing on national TV.
    /// </summary>
    public string Details { get; }
}
