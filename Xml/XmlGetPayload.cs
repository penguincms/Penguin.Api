using Penguin.Api.Shared;

namespace Penguin.Api.Xml
{
    public class XmlGetPayload : EmptyPayload
    {
        public XmlGetPayload()
        {
            this.Headers.Add("Accept", "application/Xml, text/plain, */*");
            this.Headers.Add("Content-Type", "application/Xml;charset=UTF-8");
        }
    }
}