#pragma warning disable SA1600
#pragma warning disable CS1591

using System;
using System.Text;

namespace Jellyfin.XmlTv.Entities
{
    public class XmlTvChannel : IEquatable<XmlTvChannel>
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Number { get; set; }

        public string Url { get; set; }

        public XmlTvIcon Icon { get; set; }

        /// <inheritdoc />
        public bool Equals(XmlTvChannel other)
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
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.InvariantCulture) * 17;

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Id).Append(" - ").Append(DisplayName).Append(' ');

            if (!string.IsNullOrEmpty(Url))
            {
                builder.Append(" (").Append(Url).Append(')');
            }

            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is XmlTvChannel channel && Equals(channel);
    }
}
