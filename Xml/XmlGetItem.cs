using Penguin.Api.Shared;
using Penguin.Web.Abstractions.Interfaces;

namespace Penguin.Api.Xml
{
    //Why is this even a thing?
    public class XmlGetItem : BaseGetItem<XmlGetPayload, XmlResponsePayload>
    {
        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<XmlGetPayload, XmlResponsePayload> item)
        {
            return TryCreate(request, response, "text/xml", out item);
        }
    }
}