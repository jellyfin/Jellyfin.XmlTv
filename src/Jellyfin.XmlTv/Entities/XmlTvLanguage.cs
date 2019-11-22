namespace Jellyfin.XmlTv.Entities
{
    /// <summary>
    /// Class XmlTvLanguage.
    /// </summary>
    public class XmlTvLanguage
    {
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
