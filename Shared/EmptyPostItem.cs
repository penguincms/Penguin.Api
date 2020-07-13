using Penguin.Web.Abstractions.Interfaces;

namespace Penguin.Api.Shared
{
    public class EmptyPostItem : BasePostItem<EmptyPayload, GenericResponsePayload>
    {
        public override void FillBody(string source)
        {
        }

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<EmptyPayload, GenericResponsePayload> item) => this.TryCreate(request, response, (contentType) => string.IsNullOrWhiteSpace(contentType), out item);
    }
}