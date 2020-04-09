using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Playlist;
using System;

namespace Penguin.Api.Shared
{
    public class HttpGetItem : BasePlaylistItem<EmptyPayload, GenericResponsePayload>
    {
        public override GenericResponsePayload Execute(IApiPlaylistSessionContainer Container)
        {
            this.ApplyHeaders(Container);

            return BuildResponse(Container);
        }

        public override string GetBody(IApiPlaylistSessionContainer Container, string transformedUrl)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            return Container.Client.DownloadString(transformedUrl);
        }

        public override EmptyPayload Transform(IApiPlaylistSessionContainer Container)
        {
            return null;
        }
    }
}