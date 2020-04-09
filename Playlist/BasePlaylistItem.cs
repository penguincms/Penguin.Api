using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Forms;
using Penguin.Api.Shared;
using Penguin.Extensions.Strings;
using Penguin.Json.Extensions;
using Penguin.Web.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Penguin.Api.Playlist
{
    public abstract class BasePlaylistItem<TRequest, TResponse> : IPlaylistItem<TRequest, TResponse> where TRequest : ApiPayload, new() where TResponse : ApiServerResponse, new()
    {
        private string url;
        public bool Executed { get; protected set; }
        public string Id { get; set; }
        public FormItemCollection QueryParameters { get; set; } = new FormItemCollection();
        public TRequest Request { get; set; } = new TRequest();
        IApiPayload IPlaylistItem.Request => Request;
        public TResponse Response { get; protected set; } = new TResponse();
        IApiServerResponse IPlaylistItem.Response => Response;
        public List<ITransformation> Transformations { get; set; } = new List<ITransformation>();

        public string Url
        {
            get => url;
            set
            {
                string v = value;

                if (v?.Contains("?") ?? false)
                {
                    string parameters = v.From("?");
                    v = v.To("?");

                    this.QueryParameters = new FormItemCollection(parameters);
                }

                url = v;
            }
        }

        public void ApplyHeaders(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            foreach (HttpHeader ph in Request.Headers)
            {
                List<string> keys = new List<string>();

                foreach (string k in Container.Client.Headers.Keys)
                {
                    keys.Add(k);
                }

                foreach (string ok in keys)
                {
                    if (string.Equals(ok, ph.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        Container.Client.Headers.Remove(ok);
                    }
                }

                if (ph.Mode != HeaderMode.Ignore)
                {
                    Container.Client.Headers.Add(ph.Key, ph.Value);
                }
            }
        }

        public TResponse BuildResponse(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            this.Response = Activator.CreateInstance<TResponse>();

            try
            {
                Response.Body = GetBody(Container, GetTransformedUrl(Container));
                Response.Status = ApiServerResponseStatus.Success;
            }
            catch (Exception ex)
            {
                if (ex is WebException wex)
                {
                    HttpWebResponse errorResponse = wex.Response as HttpWebResponse;
                    if (errorResponse.StatusCode != HttpStatusCode.NotFound)
                    {
                        Response.Status = ApiServerResponseStatus.Error;
                        this.Response.Body = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                        throw;
                    }
                    else
                    {
                        Response.Status = ApiServerResponseStatus.Warning;
                    }
                }
                else
                {
                    throw;
                }
            }

            if (Container.Client.ResponseHeaders != null)
            {
                foreach (string key in Container.Client.ResponseHeaders.Keys)
                {
                    Response.Headers[key] = Container.Client.ResponseHeaders[key];
                }
            }

            return Response;
        }

        public abstract TResponse Execute(IApiPlaylistSessionContainer Container);

        IApiServerResponse IPlaylistItem.Execute(IApiPlaylistSessionContainer Container) => Execute(Container);

        public abstract string GetBody(IApiPlaylistSessionContainer Container, string transformedUrl);

        public virtual string GetReplacement(string toReplace, IApiPlaylistSessionContainer Container)
        {
            string value;

            if (toReplace.StartsWith("//"))
            {
                string SourceId = toReplace.From("//").To("//");

                XPathAttributeTransformation xPathAttributeTransformation = new XPathAttributeTransformation()
                {
                    SourceAttribute = toReplace.FromLast("//"),
                    SourcePath = toReplace.From("//").From("//", true).ToLast("//")
                };

                xPathAttributeTransformation.TryGetTransformedValue(Container.PreviousResponses[SourceId], out value);
            }
            else
            {
                string sourceId = toReplace.To(".");

                string sourcePath = toReplace.From(".");

                Container.PreviousResponses[sourceId].TryGetValue(sourcePath, out value);
            }

            return value;
        }

        public virtual string GetTransformedUrl(IApiPlaylistSessionContainer Container)
        {
            bool inReplace = false;
            string transformedUrl = this.Url;

            List<string> Replacements = new List<string>();

            string replacement = string.Empty;

            for (int i = 0; i < this.Url.Length; i++)
            {
                if (this.Url[i] == '{')
                {
                    inReplace = true;
                    continue;
                }

                if (this.Url[i] == '}')
                {
                    inReplace = false;
                    if (!string.IsNullOrWhiteSpace(replacement))
                    {
                        Replacements.Add(replacement);
                        replacement = string.Empty;
                    }
                    continue;
                }

                if (inReplace)
                {
                    replacement += this.Url[i];
                }
            }

            foreach (string thisReplacement in Replacements)
            {
                transformedUrl = transformedUrl.Replace($"{{{thisReplacement}}}", GetReplacement(thisReplacement, Container));
            }

            if (QueryParameters.Any())
            {
                transformedUrl = $"{transformedUrl}?{QueryParameters}";
            }

            return transformedUrl;
        }

        public override string ToString()
        {
            return $"{Id} ({this.Url})";
        }

        public virtual TRequest Transform(IApiPlaylistSessionContainer Container)
        {
            TRequest clonedRequest = this.Request.JsonClone();

            foreach (HttpHeader header in clonedRequest.Headers)
            {
                if (header.Key.StartsWith("$"))
                {
                    header.Key = header.Key.Substring(1);

                    header.Value = GetReplacement(header.Value, Container);
                }
            }

            return clonedRequest;
        }
    }
}