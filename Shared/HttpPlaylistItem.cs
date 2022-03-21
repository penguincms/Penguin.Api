using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Extensions;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Forms;
using Penguin.Api.Playlist;
using Penguin.Extensions.Strings;
using Penguin.Json.Extensions;
using Penguin.Web.Abstractions.Interfaces;
using Penguin.Web.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;

namespace Penguin.Api.Shared
{
    public abstract class HttpPlaylistItem<TRequest, TResponse> : BasePlaylistItem, ITryGetReplacement, IHttpPlaylistItem<TRequest, TResponse> where TResponse : ApiServerResponse, new() where TRequest : ApiPayload, new()
    {
        private string url;

        [Display(Order = -500)]
        public FormItemCollection QueryParameters { get; set; } = new FormItemCollection();

        [Display(Order = -100)]
        public TRequest Request { get; set; } = new TRequest();

        IApiPayload IHttpPlaylistItem.Request => this.Request;

        [Display(Order = 1000)]
        public TResponse Response { get; protected set; } = new TResponse();

        IApiServerResponse IHttpPlaylistItem.Response => this.Response;

        [Display(Order = -1000)]
        public string Url
        {
            get
            {
                string v = this.url;

                if (this.QueryParameters.Where(k => !string.IsNullOrWhiteSpace(k.Name)).Any())
                {
                    v += $"?{this.QueryParameters}";
                }

                return v;
            }
            set
            {
                string v = value;

                if (v != null)
                {
                    int inBraces = 0;
                    int qStart = -1;

                    for (int i = 0; i < v.Length; i++)
                    {
                        if (v[i] == '{')
                        {
                            inBraces++;
                        }

                        if (inBraces == 0 && v[i] == '?')
                        {
                            qStart = i;
                        }

                        if (v[i] == '}')
                        {
                            inBraces--;
                        }
                    }

                    if (qStart >= 0)
                    {
                        string parameters = v.Substring(qStart + 1);
                        v = v.Substring(0, qStart);

                        this.QueryParameters = new FormItemCollection(parameters);
                    }
                }

                this.url = v;
            }
        }

        public void ApplyHeaders(IApiPlaylistSessionContainer Container, TRequest request)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            foreach (HttpHeader ph in request.Headers)
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

                this.ApplyHeaders(Container, clonedRequest);

                void FillHeaders(WebHeaderCollection headers)
                {
                    if (headers != null)
                    {
                        foreach (string key in headers.Keys)
                        {
                            this.Response.Headers[key] = headers[key];
                        }
                    }
                }

                try
                {
                    this.Response.Body = this.GetBody(Container, clonedRequest);
                    this.Response.Status = ApiServerResponseStatus.Success;

                    FillHeaders(Container.Client.ResponseHeaders);
                }
                catch (Exception ex)
                {
                    this.Response.Exception = ex;

                    if (ex is WebException wex)
                    {
                        HttpWebResponse errorResponse = wex.Response as HttpWebResponse;

                        if (errorResponse is null || errorResponse.StatusCode != HttpStatusCode.NotFound)
                        {
                            this.Response.Status = ApiServerResponseStatus.Error;

                            try
                            {
                                using (StreamReader sr = new StreamReader(wex.Response.GetResponseStream()))
                                {
                                    this.Response.Body = sr.ReadToEnd();
                                }
                            } catch(Exception iex)
                            {
                                this.Response.Body = "Error retrieving response body: " + iex.Message;
                            }
                        }
                        else
                        {
                            this.Response.Status = ApiServerResponseStatus.Warning;
                        }

                        if (errorResponse != null)
                        {
                            FillHeaders(errorResponse.Headers);
                        }
                    }
                    else
                    {
                        this.Response.Status = ApiServerResponseStatus.Error;
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Response.Exception = ex;
                this.Response.Status = ApiServerResponseStatus.Error;
            }

            return ApiServerInteraction.Create(clonedRequest, this.Response, this.Id);
        }

        public abstract IApiServerInteraction<TRequest, TResponse> Execute(IApiPlaylistSessionContainer Container);

        void IPlaylistItem.Execute(IApiPlaylistSessionContainer Container) => this.Execute(Container);

        IApiServerInteraction IPlaylistItem<IApiServerInteraction>.Execute(IApiPlaylistSessionContainer Container) => this.Execute(Container);

        public abstract string GetBody(IApiPlaylistSessionContainer Container, TRequest request);

        public virtual string GetTransformedUrl(IApiPlaylistSessionContainer Container)
        {
            string inputString = this.url;

            string outputString = inputString;

            while (outputString.Contains("}"))
            {
                int firstClose = outputString.IndexOf("}", StringComparison.OrdinalIgnoreCase);

                string temp = outputString.Substring(0, firstClose);

                int lastOpen = temp.LastIndexOf("{", StringComparison.OrdinalIgnoreCase);

                temp = temp.Substring(lastOpen + 1);

                outputString = outputString.Replace($"{{{temp}}}", this.FindReplacement(temp, Container));
            }

            if (this.QueryParameters.Any())
            {
                FormItemCollection newParams = new FormItemCollection();

                foreach (FormItem queryParameter in this.QueryParameters)
                {
                    newParams.Add(queryParameter.Name, queryParameter.Value);
                }

                foreach (FormItem queryParameter in newParams)
                {
                    if (queryParameter.Name.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                    {
                        queryParameter.Name = queryParameter.Name.Substring(1);

                        if (this.TryGetReplacement(queryParameter.Value, Container, out object newValue))
                        {
                            queryParameter.Value = newValue?.ToString();
                        }
                    }
                }

                outputString += $"?{newParams}";
            }

            return outputString;
        }

        public override void Reset()
        {
            this.Response = new TResponse();
            this.Executed = false;
        }

        public override string ToString() => $"{this.Id} ({this.Url})";

        public virtual TRequest Transform(IApiPlaylistSessionContainer Container)
        {
            TRequest clonedRequest = this.Request.JsonClone();

            clonedRequest.Url = this.GetTransformedUrl(Container);

            foreach (HttpHeader header in clonedRequest.Headers)
            {
                if (header.Key.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                {
                    header.Key = header.Key.Substring(1);

                    if (this.TryGetReplacement(header.Value, Container, out object v))
                    {
                        header.Value = v.ToString();
                    }
                }
            }

            return clonedRequest;
        }

        public abstract bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<TRequest, TResponse> item);

        bool IHttpPlaylistItem<TRequest, TResponse>.TryCreate(IHttpServerRequest request, IHttpServerResponse response, out IHttpPlaylistItem<TRequest, TResponse> item)
        {
            bool r = this.TryCreate(request, response, out HttpPlaylistItem<TRequest, TResponse> i);
            item = i;
            return r;
        }

        bool IHttpPlaylistItem.TryCreate(IHttpServerRequest request, IHttpServerResponse response, out IHttpPlaylistItem item)
        {
            bool r = this.TryCreate(request, response, out HttpPlaylistItem<TRequest, TResponse> i);
            item = i;
            return r;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
        public virtual bool TryGetReplacement(string toReplace, IApiPlaylistSessionContainer Container, out object value)
        {
            if (toReplace is null)
            {
                throw new ArgumentNullException(nameof(toReplace));
            }

            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            if (toReplace.StartsWith("//", StringComparison.OrdinalIgnoreCase))
            {
                string SourceId = toReplace.From("//").To("//");

                XPathAttributeTransformation xPathAttributeTransformation = new XPathAttributeTransformation()
                {
                    SourceAttribute = toReplace.FromLast("//"),
                    SourcePath = toReplace.From("//").From("//", true).ToLast("//")
                };

                return xPathAttributeTransformation.TryGetTransformedValue(Container.Interactions.Responses[SourceId], out value);
            }
            else if (toReplace.StartsWith("@", StringComparison.OrdinalIgnoreCase))
            {
                value = Container.SessionObjects.Get(toReplace.Substring(1));
                return true;
            }
            else if (toReplace.StartsWith("regex(", StringComparison.OrdinalIgnoreCase))
            {
                string val = toReplace.From("(").ToLast(")");

                string[] parameters = val.SplitQuotedString().ToArray();

                string sourceId = parameters[0].Trim();
                string match = parameters[1].Trim();
                string group = parameters[2].Trim();
                string expression = parameters[3].Trim();

                RegexTransformation rt = new RegexTransformation()
                {
                    SourceId = sourceId,
                    RegexExpression = expression,
                    Group = int.Parse(group),
                    MatchIndex = int.Parse(match)
                };

                if (Container.Interactions.Responses.TryGetValue(sourceId, out IApiServerResponse response))
                {
                    return rt.TryGetTransformedValue(response, out value);
                }

                value = null;
                return false;
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

        protected void SetupHttpPlaylistItem(HttpPlaylistItem<TRequest, TResponse> item, IHttpServerRequest request, IHttpServerResponse response)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (response is null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            item.Enabled = true;

            foreach (KeyValuePair<string, string> header in request.Headers)
            {
                item.Request.Headers.Add(header.Key, header.Value);
            }

            item.Url = request.Url;

            if (response != null)
            {
                foreach (KeyValuePair<string, string> header in response.Headers)
                {
                    item.Response.Headers.Add(header.Key, header.Value);
                }

                item.Response.Body = response.BodyText;
            }
        }
    }
}
