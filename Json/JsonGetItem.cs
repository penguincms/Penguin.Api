using Penguin.Api.Shared;
using Penguin.Web.Abstractions.Interfaces;

namespace Penguin.Api.Json
{
    //Why is this even a thing?
    public class JsonGetItem : BaseGetItem<JsonGetPayload, JsonResponsePayload>
    {
        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<JsonGetPayload, JsonResponsePayload> item) => this.TryCreate(request, response, "application/json", out item);
    }
}