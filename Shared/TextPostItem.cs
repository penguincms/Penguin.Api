using Penguin.Api.PostBody;
using Penguin.Web.Abstractions.Interfaces;

namespace Penguin.Api.Shared
{
    public class TextPostItem : BasePostItem<TextPostPayload, GenericResponsePayload>
    {
        public override void FillBody(string source)
        {
            Request ??= new TextPostPayload();
            Request.Body = new TextPostBody();
            Request.Body.Convert(source);
        }

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<TextPostPayload, GenericResponsePayload> item)
        {
            return TryCreate(request, response, "text/plain", out item);
        }
    }
}