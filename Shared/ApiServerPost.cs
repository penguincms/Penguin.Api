using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Playlist;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{
    public abstract class ApiServerPost<TRequest, TResponse> : BasePlaylistItem<TRequest, TResponse>, IPostItem where TResponse : ApiServerResponse, new() where TRequest : ApiPayload, new()
    {
        public override TResponse Execute(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            ApplyHeaders(Container);

            return BuildResponse(Container);
        }

        public abstract void FillBody(string source);

        public override string GetBody(IApiPlaylistSessionContainer Container, string transformedUrl)
        {
            return Container.Client.UploadString(transformedUrl, this.Transform(Container).ToString());
        }

        public override TRequest Transform(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            TRequest clonedRequest = base.Transform(Container);

            foreach (ITransformation transformation in Transformations)
            {
                foreach (KeyValuePair<string, IApiServerResponse> response in Container.PreviousResponses)
                {
                    transformation.Transform(response, clonedRequest);
                }
            }

            return clonedRequest;
        }
    }
}