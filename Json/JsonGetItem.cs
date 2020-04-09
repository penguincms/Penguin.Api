using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Playlist;
using Penguin.Api.Shared;

namespace Penguin.Api.Json
{
    //Why is this even a thing?
    public class JsonGetItem : BasePlaylistItem<EmptyPayload, JsonResponsePayload>
    {
        public override JsonResponsePayload Execute(IApiPlaylistSessionContainer Container)
        {
            this.ApplyHeaders(Container);

            return BuildResponse(Container);
        }

        public override string GetBody(IApiPlaylistSessionContainer Container, string transformedUrl) => Container.Client.DownloadString(transformedUrl);

        public override EmptyPayload Transform(IApiPlaylistSessionContainer Container)
        {
            return null;
        }
    }
}