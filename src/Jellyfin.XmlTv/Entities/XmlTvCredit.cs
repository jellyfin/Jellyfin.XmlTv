namespace Jellyfin.XmlTv.Entities
{
    public class XmlTvCredit
    {
        public XmlTvCreditType Type { get; set; }
        public string Name { get; set; }

        public override string ToString()
            => string.Format("{0} - ({1})", Name, Type);
    }
}
