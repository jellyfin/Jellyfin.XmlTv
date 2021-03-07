using System.Text;

namespace Jellyfin.XmlTv.Entities
{
    /// <summary>
    /// Describes the rating (certification) applied to a program.
    /// </summary>
    public class XmlTvRating
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlTvRating"/> class.
        /// </summary>
        /// <param name="value">The rating.</param>
        public XmlTvRating(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the literal name of the rating system.
        /// </summary>
        /// Example: MPAA
        public string? System { get; set; }

        /// <summary>
        /// Gets or sets the rating using the system specificed.
        /// </summary>
        // Example: TV-14
        public string Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder(Value);
            if (!string.IsNullOrEmpty(System))
            {
                builder.Append(" (").Append(System).Append(')');
            }

            return builder.ToString();
        }
    }
}
