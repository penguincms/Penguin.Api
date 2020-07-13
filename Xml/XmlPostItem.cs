using Penguin.Api.Shared;
using Penguin.Web.Abstractions.Interfaces;

namespace Penguin.Api.Xml
{
    public class XmlPostItem : BasePostItem<XmlPostPayload, XmlResponsePayload>
    {
        public override void FillBody(string source)
        {
            this.Request = this.Request ?? new XmlPostPayload();
            this.Request.Body = source;
        }

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<XmlPostPayload, XmlResponsePayload> item) => this.TryCreate(request, response, "text/xml", out item);
    }
}