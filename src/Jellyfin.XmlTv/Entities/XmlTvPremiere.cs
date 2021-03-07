#pragma warning disable CS1591

namespace Jellyfin.XmlTv.Entities
{
    /*
    <premiere lang="en">
      First showing on national terrestrial TV
    </premiere>
    */
    public class XmlTvPremiere
    {
        public XmlTvPremiere(string details)
        {
            Details = details;
        }

        public string Details { get; }
    }
}
