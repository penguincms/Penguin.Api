using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Playlist;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{


    public abstract class ApiServerPost<TRequest, TResponse> : BasePlaylistItem<TRequest, TResponse>, IPostItem where TResponse : ApiServerResponse, new() where TRequest : ApiPayload, new()
    {
        public PostMethod Method { get; set; } = PostMethod.POST;

        public override IApiServerInteraction<TRequest, TResponse> Execute(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            ApplyHeaders(Container);

            return BuildResponse(Container);
        }

        public abstract void FillBody(string source);

        
        public override string GetBody(IApiPlaylistSessionContainer Container, TRequest request)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return Container.Client.UploadString(request.Url, Method.ToString(), request.ToString());
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
                foreach (KeyValuePair<string, IApiServerResponse> response in Container.Interactions.Responses)
                {
                    transformation.Transform(response, clonedRequest);
                }
            }

            return clonedRequest;
        }
    }
}