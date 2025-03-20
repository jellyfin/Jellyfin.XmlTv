namespace Jellyfin.XmlTv.Entities;

/// <summary>
/// Language class.
/// </summary>
public class XmlTvLanguage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlTvLanguage"/> class.
    /// </summary>
    /// <param name="name">The language name.</param>
    /// <param name="relevance">The relevance (number of occurrences) of the language.</param>
    public XmlTvLanguage(string name, int relevance)
    {
        Name = name;
        Relevance = relevance;
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the relevance (number of occurrences) of the language, can be used to order (desc).
    /// </summary>
    public int Relevance { get; }
}
