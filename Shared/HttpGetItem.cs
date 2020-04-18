using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Playlist;
using System;

namespace Penguin.Api.Shared
{
    public class HttpGetItem : BasePlaylistItem<EmptyPayload, GenericResponsePayload>
    {
        public override IApiServerInteraction<EmptyPayload, GenericResponsePayload> Execute(IApiPlaylistSessionContainer Container)
        {
            this.ApplyHeaders(Container);

            return BuildResponse(Container);
        }

        public override string GetBody(IApiPlaylistSessionContainer Container, EmptyPayload request)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            return Container.Client.DownloadString(request.Url);
        }
    }
}