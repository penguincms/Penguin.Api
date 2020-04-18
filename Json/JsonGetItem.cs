using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Playlist;
using Penguin.Api.Shared;

namespace Penguin.Api.Json
{
    //Why is this even a thing?
    public class JsonGetItem : BasePlaylistItem<JsonGetPayload, JsonResponsePayload>
    {
        public override IApiServerInteraction<JsonGetPayload, JsonResponsePayload> Execute(IApiPlaylistSessionContainer Container)
        {
            this.ApplyHeaders(Container);

            return BuildResponse(Container);
        }

        public override string GetBody(IApiPlaylistSessionContainer Container, JsonGetPayload request)
        {
            if (Container is null)
            {
                throw new System.ArgumentNullException(nameof(Container));
            }

            if (request is null)
            {
                throw new System.ArgumentNullException(nameof(request));
            }

            return Container.Client.DownloadString(request.Url);
        }

    }
}