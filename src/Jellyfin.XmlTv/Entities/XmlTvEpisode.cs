#pragma warning disable SA1600
#pragma warning disable CS1591

using System.Text;

namespace Jellyfin.XmlTv.Entities
{
    public class XmlTvEpisode
    {
        public int? Series { get; set; }

        public int? SeriesCount { get; set; }

        public int? Episode { get; set; }

        public int? EpisodeCount { get; set; }

        public string Title { get; set; }

        public int? Part { get; set; }

        public int? PartCount { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Series.HasValue || SeriesCount.HasValue)
            {
                builder.Append("Series ");
                if (Series.HasValue)
                {
                    builder.Append(Series);
                }
                else
                {
                    builder.Append('?');
                }

                if (SeriesCount.HasValue)
                {
                    builder.Append(" of ").Append(SeriesCount);
                }
            }

            if (Episode.HasValue || EpisodeCount.HasValue)
            {
                builder.Append(builder.Length > 0 ? ", " : string.Empty);
                builder.Append("Episode ");

                if (Episode.HasValue)
                {
                    builder.Append(Episode);
                }
                else
                {
                    builder.Append('?');
                }

                if (EpisodeCount.HasValue)
                {
                    builder.Append(" of ").Append(EpisodeCount);
                }
            }

            if (Part.HasValue || PartCount.HasValue)
            {
                builder.Append(builder.Length > 0 ? ", " : string.Empty);
                builder.Append("Part ");
                if (Part.HasValue)
                {
                    builder.Append(Part);
                }
                else
                {
                    builder.Append('?');
                }

                if (PartCount.HasValue)
                {
                    builder.Append(" of ").Append(PartCount);
                }
            }

            return builder.ToString();
        }
    }
}
