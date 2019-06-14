namespace Jellyfin.XmlTv.Entities
{
    public class XmlTvLanguage
    {
        /// <summary>
        /// The name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The relevance (number of occurrences) of the language, can be used to order (desc)
        /// </summary>
        public int Relevance { get; set; }
    }
}
