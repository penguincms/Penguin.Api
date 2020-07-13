using Penguin.Api.Abstractions.Interfaces;
using Penguin.Web.Abstractions.Interfaces;
using System;

namespace Penguin.Api.Shared
{
    public abstract class BaseGetItem<TRequest, TResponse> : HttpPlaylistItem<TRequest, TResponse> where TRequest : ApiPayload, new() where TResponse : ApiServerResponse, new()
    {
        public override IApiServerInteraction<TRequest, TResponse> Execute(IApiPlaylistSessionContainer Container)
        {
            this.ApplyHeaders(Container);

            return this.BuildResponse(Container);
        }

        public override string GetBody(IApiPlaylistSessionContainer Container, TRequest request)
        {
            return Container is null
                ? throw new System.ArgumentNullException(nameof(Container))
                : request is null ? throw new System.ArgumentNullException(nameof(request)) : Container.Client.DownloadString(request.Url);
        }

        public bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, Func<string, bool> contentTypeCheck, out HttpPlaylistItem<TRequest, TResponse> item)
        {
            if (request is null)
            {
                throw new System.ArgumentNullException(nameof(request));
            }

            if (contentTypeCheck is null)
            {
                throw new ArgumentNullException(nameof(contentTypeCheck));
            }

            string checkContentType = request.ContentType;

            if (checkContentType is null)
            {
                if (response?.ContentType is null)
                {
                    item = null;
                    return false;
                }
                else
                {
                    checkContentType = response.ContentType;
                }
            }

            if (request.Method == "GET" && contentTypeCheck(checkContentType))
            {
                HttpPlaylistItem<TRequest, TResponse> bi = Activator.CreateInstance(this.GetType()) as HttpPlaylistItem<TRequest, TResponse>;

                item = bi;

                base.SetupHttpPlaylistItem(item, request, response);

                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }

        public bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, string targetContentType, out HttpPlaylistItem<TRequest, TResponse> item) => this.TryCreate(request, response, (ContentType) => ContentType.StartsWith(targetContentType, StringComparison.OrdinalIgnoreCase), out item);
    }
}