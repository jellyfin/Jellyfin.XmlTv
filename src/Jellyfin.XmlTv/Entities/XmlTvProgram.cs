#pragma warning disable CS1591, CA1002

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Jellyfin.XmlTv.Entities
{
    public class XmlTvProgram : IEquatable<XmlTvProgram>
    {
        public XmlTvProgram(string channelId)
        {
            ChannelId = channelId;
            Credits = new List<XmlTvCredit>();
            Categories = new List<string>();
            Countries = new List<string>();
            Episode = null;

            ProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            SeriesProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string ChannelId { get; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? ProgramId { get; set; }

        public string? Quality { get; set; }

        public List<string> Categories { get; }

        public List<string> Countries { get; }

        public DateTimeOffset? PreviouslyShown { get; set; }

        public bool IsPreviouslyShown { get; set; }

        public bool IsNew { get; set; }

        public bool IsLive { get; set; }

        public DateTimeOffset? CopyrightDate { get; set; }

        public XmlTvEpisode? Episode { get; set; }

        public List<XmlTvCredit> Credits { get; }

        public XmlTvRating? Rating { get; set; }

        public float? StarRating { get; set; }

        public XmlTvIcon? Icon { get; set; }

        public XmlTvPremiere? Premiere { get; set; }

        public Dictionary<string, string> ProviderIds { get; }

        public Dictionary<string, string> SeriesProviderIds { get; }

        /// <inheritdoc />
        public bool Equals(XmlTvProgram? other)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // If the other is null then return false
            if (other == null)
            {
                return false;
            }

            // Return true if the fields match:
            return ChannelId == other.ChannelId
                && StartDate == other.StartDate
                && EndDate == other.EndDate;
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => (ChannelId.GetHashCode(StringComparison.Ordinal) * 17) + (StartDate.GetHashCode() * 17) + (EndDate.GetHashCode() * 17);

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, "ChannelId: \t\t{0}\r\n", ChannelId);
            builder.AppendFormat(CultureInfo.InvariantCulture, "Title: \t\t{0}\r\n", Title);
            builder.AppendFormat(CultureInfo.InvariantCulture, "StartDate: \t\t{0}\r\n", StartDate);
            builder.AppendFormat(CultureInfo.InvariantCulture, "EndDate: \t\t{0}\r\n", EndDate);
            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is XmlTvProgram tvProgram && Equals(tvProgram);
    }
}
