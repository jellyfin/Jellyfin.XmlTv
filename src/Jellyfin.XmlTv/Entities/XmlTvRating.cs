using System.Text;

namespace Jellyfin.XmlTv.Entities
{
    /// <summary>
    /// Describes the rating (certification) applied to a program.
    /// </summary>
    public class XmlTvRating
    {
        /// <summary>
        /// Gets or sets the literal name of the rating system.
        /// </summary>
        /// Example: MPAA
        public string System { get; set; }

        /// <summary>
        /// Gets or setsthe rating using the system specificed.
        /// </summary>
        // Example: TV-14
        public string Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Value))
            {
                builder.Append(Value);
            }

            if (!string.IsNullOrEmpty(System))
            {
                builder.Append(" (").Append(System).Append(')');
            }

            return builder.ToString();
        }
    }
}
