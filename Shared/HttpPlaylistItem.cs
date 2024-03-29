using Loxifi;
using Loxifi.Extensions.StringExtensions;
using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Extensions;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Forms;
using Penguin.Api.Playlist;
using Penguin.Extensions.String;
using Penguin.Json.Extensions;
using Penguin.Web.Abstractions.Interfaces;
using Penguin.Web.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using StringSplitOptions = Loxifi.Extensions.StringExtensions.StringSplitOptions;

namespace Penguin.Api.Shared
{
    public abstract class HttpPlaylistItem<TRequest, TResponse> : BasePlaylistItem, ITryGetReplacement, IHttpPlaylistItem<TRequest, TResponse> where TResponse : ApiServerResponse, new() where TRequest : ApiPayload, new()
    {
        private string url;

        [Display(Order = -500)]
        public FormItemCollection QueryParameters { get; set; } = new FormItemCollection();

        [Display(Order = -100)]
        public TRequest Request { get; set; } = new TRequest();

        IApiPayload IHttpPlaylistItem.Request => Request;

        [Display(Order = 1000)]
        public TResponse Response { get; protected set; } = new TResponse();

        IApiServerResponse IHttpPlaylistItem.Response => Response;

        [Display(Order = -1000)]
        public string Url
        {
            get
            {
                string v = url;

                if (QueryParameters.Where(k => !string.IsNullOrWhiteSpace(k.Name)).Any())
                {
                    v += $"?{QueryParameters}";
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
                        string parameters = v[(qStart + 1)..];
                        v = v[..qStart];

                        QueryParameters = new FormItemCollection(parameters);
                    }
                }

                url = v;
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
                List<string> keys = new();

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
            Response = Activator.CreateInstance<TResponse>();

            try
            {
                clonedRequest = Transform(Container);

                ApplyHeaders(Container, clonedRequest);

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
                    Response.Exception = ex;

                    if (ex is WebException wex)
                    {
                        HttpWebResponse errorResponse = wex.Response as HttpWebResponse;

                        if (errorResponse is null || errorResponse.StatusCode != HttpStatusCode.NotFound)
                        {
                            Response.Status = ApiServerResponseStatus.Error;

                            try
                            {
                                using StreamReader sr = new(wex.Response.GetResponseStream());
                                Response.Body = sr.ReadToEnd();
                            }
                            catch (Exception iex)
                            {
                                Response.Body = "Error retrieving response body: " + iex.Message;
                            }
                        }
                        else
                        {
                            Response.Status = ApiServerResponseStatus.Warning;
                        }

                        if (errorResponse != null)
                        {
                            FillHeaders(errorResponse.Headers);
                        }
                    }
                    else
                    {
                        Response.Status = ApiServerResponseStatus.Error;
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Exception = ex;
                Response.Status = ApiServerResponseStatus.Error;
            }

            return ApiServerInteraction.Create(clonedRequest, Response, Id);
        }

        public abstract IApiServerInteraction<TRequest, TResponse> Execute(IApiPlaylistSessionContainer Container);

        void IPlaylistItem.Execute(IApiPlaylistSessionContainer Container)
        {
            _ = Execute(Container);
        }

        IApiServerInteraction IPlaylistItem<IApiServerInteraction>.Execute(IApiPlaylistSessionContainer Container)
        {
            return Execute(Container);
        }

        public abstract string GetBody(IApiPlaylistSessionContainer Container, TRequest request);

        public virtual string GetTransformedUrl(IApiPlaylistSessionContainer Container)
        {
            string inputString = url;

            string outputString = inputString;

            while (outputString.Contains('}'))
            {
                int firstClose = outputString.IndexOf("}", StringComparison.OrdinalIgnoreCase);

                string temp = outputString[..firstClose];

                int lastOpen = temp.LastIndexOf("{", StringComparison.OrdinalIgnoreCase);

                temp = temp[(lastOpen + 1)..];

                outputString = outputString.Replace($"{{{temp}}}", this.FindReplacement(temp, Container));
            }

            if (QueryParameters.Any())
            {
                FormItemCollection newParams = new();

                foreach (FormItem queryParameter in QueryParameters)
                {
                    newParams.Add(queryParameter.Name, queryParameter.Value);
                }

                foreach (FormItem queryParameter in newParams)
                {
                    if (queryParameter.Name.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                    {
                        queryParameter.Name = queryParameter.Name[1..];

                        if (TryGetReplacement(queryParameter.Value, Container, out object newValue))
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
            Response = new TResponse();
            Executed = false;
        }

        public override string ToString()
        {
            return $"{Id} ({Url})";
        }

        public virtual TRequest Transform(IApiPlaylistSessionContainer Container)
        {
            TRequest clonedRequest = Request.JsonClone();

            clonedRequest.Url = GetTransformedUrl(Container);

            foreach (HttpHeader header in clonedRequest.Headers)
            {
                if (header.Key.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                {
                    header.Key = header.Key[1..];

                    if (TryGetReplacement(header.Value, Container, out object v))
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
            bool r = TryCreate(request, response, out HttpPlaylistItem<TRequest, TResponse> i);
            item = i;
            return r;
        }

        bool IHttpPlaylistItem.TryCreate(IHttpServerRequest request, IHttpServerResponse response, out IHttpPlaylistItem item)
        {
            bool r = TryCreate(request, response, out HttpPlaylistItem<TRequest, TResponse> i);
            item = i;
            return r;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>")]
        public virtual bool TryGetReplacement(string toReplace, IApiPlaylistSessionContainer Container, out object v)
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

                XPathAttributeTransformation xPathAttributeTransformation = new()
                {
                    SourceAttribute = toReplace.FromLast("//"),
                    SourcePath = toReplace.From("//").From("//", true).ToLast("//")
                };

                return xPathAttributeTransformation.TryGetTransformedValue(Container.Interactions.Responses[SourceId], out v);
            }
            else if (toReplace.StartsWith("@", StringComparison.OrdinalIgnoreCase))
            {
                v = Container.SessionObjects.Get(toReplace[1..]);
                return true;
            }
            else if (toReplace.StartsWith("regex(", StringComparison.OrdinalIgnoreCase))
            {
                string val = toReplace.From("(").ToLast(")");

                string[] parameters = val.Split(new StringSplitOptions()).ToArray();

                string sourceId = parameters[0].Trim();
                string match = parameters[1].Trim();
                string group = parameters[2].Trim();
                string expression = parameters[3].Trim();

                RegexTransformation rt = new()
                {
                    SourceId = sourceId,
                    RegexExpression = expression,
                    Group = int.Parse(group),
                    MatchIndex = int.Parse(match)
                };

                if (Container.Interactions.Responses.TryGetValue(sourceId, out IApiServerResponse response))
                {
                    return rt.TryGetTransformedValue(response, out v);
                }

                v = null;
                return false;
            }
            else
            {
                if (toReplace.Contains('.'))
                {
                    string sourceId = toReplace.To(".");

                    string sourcePath = toReplace.From(".");

                    if (Container.Interactions.Responses.TryGetValue(sourceId, out IApiServerResponse response))
                    {
                        return response.TryGetValue(sourcePath, out v);
                    }
                }

                v = null;
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