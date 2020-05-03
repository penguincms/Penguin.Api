using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Playlist;
using Penguin.Web.Abstractions.Interfaces;
using Penguin.Web.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.Shared
{
    public static class BasePostItem
    {
        public static bool IsSupportedMethod(string method)
        {
            return MethodStrings.Any(m => string.Equals(method, m, StringComparison.OrdinalIgnoreCase));
        }
        public static IEnumerable<string> MethodStrings
        {
            get
            {
                foreach (PostMethod m in Methods)
                {
                    yield return m.ToString();
                }
            }
        }

        public static IEnumerable<PostMethod> Methods
        {
            get
            {
                foreach (PostMethod m in Enum.GetValues(typeof(PostMethod)))
                {
                    yield return m;
                }
            }
        }
    }
    public abstract class BasePostItem<TRequest, TResponse> : HttpPlaylistItem<TRequest, TResponse>, IPostItem where TResponse : ApiServerResponse, new() where TRequest : ApiPayload, new()
    {
        public PostMethod Method { get; set; } = PostMethod.POST;
        public bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, string targetContentType, out HttpPlaylistItem<TRequest, TResponse> item)  {
            
            if(request?.ContentType is null)
            {
                item = null;
                return false;
            }

            return TryCreate(request, response, (contentType) => contentType.StartsWith(targetContentType, StringComparison.OrdinalIgnoreCase), out item);
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

            if (BasePostItem.IsSupportedMethod(request.Method) && contentTypeCheck(request.ContentType))
            {
                BasePostItem<TRequest, TResponse> bi = Activator.CreateInstance(this.GetType()) as BasePostItem<TRequest, TResponse>;

                item = bi as HttpPlaylistItem<TRequest, TResponse>;

                bi.FillBody(request.BodyText);

                bi.Method = BasePostItem.Methods.Single(m => m.ToString() == request.Method);

                base.SetupHttpPlaylistItem(item, request, response);

                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }



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