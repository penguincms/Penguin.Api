using Penguin.Api.Shared;

namespace Penguin.Api.Xml
{
    public class XmlGetPayload : EmptyPayload
    {
        public XmlGetPayload()
        {
            Headers.Add("Accept", "application/Xml, text/plain, */*");
            Headers.Add("Content-Type", "application/Xml;charset=UTF-8");
        }
    }
}