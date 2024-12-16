#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Text;
using Jellyfin.XmlTv.Interfaces;

namespace Jellyfin.XmlTv.Entities
{
    public class XmlTvChannel : IEquatable<XmlTvChannel>, IHasIcons, IHasUrls
    {
        public XmlTvChannel(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public string? DisplayName { get; set; }

        public string? Number { get; set; }

        public List<XmlTvUrl>? Urls { get; set; }

        public List<XmlTvIcon>? Icons { get; set; }

        /// <inheritdoc />
        public bool Equals(XmlTvChannel? other)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // If the other is null then return false
            if (other is null)
            {
                return false;
            }

            // Return true if the fields match:
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => Id.GetHashCode(StringComparison.Ordinal) * 17;

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Id).Append(" - ").Append(DisplayName).Append(' ');

            return builder.ToString();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is XmlTvChannel channel && Equals(channel);
    }
}
