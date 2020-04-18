using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Forms;
using Penguin.Api.ObjectArrays;
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
            get {
                string v = url;

                if(this.QueryParameters.Where(k => !string.IsNullOrWhiteSpace(k.Name)).Any())
                {
                    v += $"?{QueryParameters}";
                }

                return v;

            }
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

        public bool Enabled { get; set; } = true;

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

        public IApiServerInteraction<TRequest, TResponse> BuildResponse(IApiPlaylistSessionContainer Container)
        {

            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            TRequest clonedRequest = null;
            this.Response = Activator.CreateInstance<TResponse>();

            try
            {
                clonedRequest = this.Transform(Container);

                void FillHeaders(WebHeaderCollection headers)
                {
                    if (headers != null)
                    {
                        foreach (string key in headers.Keys)
                        {
                            Response.Headers[key] = headers[key];
                        }
                    }
                }

                try
                {
                    Response.Body = GetBody(Container, clonedRequest);
                    Response.Status = ApiServerResponseStatus.Success;

                    FillHeaders(Container.Client.ResponseHeaders);
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

                        }
                        else
                        {
                            Response.Status = ApiServerResponseStatus.Warning;
                        }

                        FillHeaders(errorResponse.Headers);
                    }
                    else
                    {
                        Response.Status = ApiServerResponseStatus.Error;
                        throw;
                    }
                }

            } catch(Exception)
            {
                Response.Status = ApiServerResponseStatus.Error;
            }

            return ApiServerInteraction.Create(clonedRequest, Response, Id);
        }

        public abstract IApiServerInteraction<TRequest, TResponse> Execute(IApiPlaylistSessionContainer Container);

        IApiServerInteraction IPlaylistItem.Execute(IApiPlaylistSessionContainer Container) => Execute(Container);

        public abstract string GetBody(IApiPlaylistSessionContainer Container, TRequest request);

        public virtual bool TryGetReplacement(string toReplace, IApiPlaylistSessionContainer Container, out string value)
        {
            if (toReplace is null)
            {
                throw new ArgumentNullException(nameof(toReplace));
            }

            if (toReplace.StartsWith("//"))
            {
                string SourceId = toReplace.From("//").To("//");

                XPathAttributeTransformation xPathAttributeTransformation = new XPathAttributeTransformation()
                {
                    SourceAttribute = toReplace.FromLast("//"),
                    SourcePath = toReplace.From("//").From("//", true).ToLast("//")
                };

                return xPathAttributeTransformation.TryGetTransformedValue(Container.Interactions.Responses[SourceId], out value);
            } else if(toReplace.StartsWith("@"))
            {
                value = Container.SessionObjects.Get(toReplace.Substring(1));
                return true;
            }
            else
            {
                if (toReplace.Contains("."))
                {
                    string sourceId = toReplace.To(".");

                    string sourcePath = toReplace.From(".");

                    if (Container.Interactions.Responses.TryGetValue(sourceId, out IApiServerResponse response))
                    {
                        return response.TryGetValue(sourcePath, out value);
                    }
                }

                value = null;
                return false;
            }
        }

        public virtual string GetTransformedUrl(IApiPlaylistSessionContainer Container)
        {
            bool inReplace = false;
            string transformedUrl = this.Url;

            List<Transformation> Replacements = new List<Transformation>();

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
                        Replacements.Add(new Transformation(replacement));
                        replacement = string.Empty;
                    }
                    continue;
                }

                if (inReplace)
                {
                    replacement += this.Url[i];
                }
            }

            foreach (Transformation thisReplacement in Replacements)
            {

                if (TryGetReplacement(thisReplacement.Value, Container, out string newValue) && (newValue != null || !thisReplacement.Required))
                {
                    transformedUrl = transformedUrl.Replace($"{{{thisReplacement.Value}}}", newValue);
                } else
                {
                    throw new Exception($"Required transformation {newValue} not found");
                }
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

            clonedRequest.Url = this.GetTransformedUrl(Container);

            foreach (HttpHeader header in clonedRequest.Headers)
            {
                if (header.Key.StartsWith("$"))
                {
                    header.Key = header.Key.Substring(1);

                    if (TryGetReplacement(header.Value, Container, out string v))
                    {
                        header.Value = v;
                    }
                }
            }

            return clonedRequest;
        }

        public virtual void Reset()
        {
            this.Response = new TResponse();
            Executed = false;
        }
    }
}