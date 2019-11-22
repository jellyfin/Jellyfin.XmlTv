#pragma warning disable SA1600
#pragma warning disable CS1591

using System.Globalization;

namespace Jellyfin.XmlTv.Entities
{
    public class XmlTvCredit
    {
        public XmlTvCreditType Type { get; set; }

        public string Name { get; set; }

        /// <inheritdoc />
        public override string ToString()
            => string.Format(
                CultureInfo.InvariantCulture,
                "{0} - ({1})",
                Name,
                Type);
    }
}
