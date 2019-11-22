#pragma warning disable SA1600
#pragma warning disable CS1591

using System.Text;

namespace Jellyfin.XmlTv.Entities
{
    public class XmlTvIcon
    {
        public string Source { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("Source: ").Append(Source);
            if (Width.HasValue)
            {
                builder.Append(", Width: ").Append(Width);
            }

            if (Height.HasValue)
            {
                builder.Append(", Height: ").Append(Height);
            }

            return builder.ToString();
        }
    }
}
