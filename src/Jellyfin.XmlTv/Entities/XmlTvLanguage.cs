namespace Jellyfin.XmlTv.Entities
{
    /// <summary>
    /// Class XmlTvLanguage.
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
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the relevance (number of occurrences) of the language, can be used to order (desc).
        /// </summary>
        public int Relevance { get; set; }
    }
}
