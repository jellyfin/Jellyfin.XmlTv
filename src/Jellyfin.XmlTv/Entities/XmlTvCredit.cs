#pragma warning disable CS1591

using System.Globalization;

namespace Jellyfin.XmlTv.Entities
{
    public class XmlTvCredit
    {
        public XmlTvCredit(XmlTvCreditType type, string name)
        {
            Type = type;
            Name = name;
        }

        public XmlTvCreditType Type { get; }

        public string Name { get; }

        /// <inheritdoc />
        public override string ToString()
            => string.Format(
                CultureInfo.InvariantCulture,
                "{0} - ({1})",
                Name,
                Type);
    }
}
