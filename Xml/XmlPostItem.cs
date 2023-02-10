using Penguin.Api.Shared;
using Penguin.Web.Abstractions.Interfaces;

namespace Penguin.Api.Xml
{
    public class XmlPostItem : BasePostItem<XmlPostPayload, XmlResponsePayload>
    {
        public override void FillBody(string source)
        {
            Request ??= new XmlPostPayload();
            Request.Body = source;
        }

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<XmlPostPayload, XmlResponsePayload> item)
        {
            return TryCreate(request, response, "text/xml", out item);
        }
    }
}