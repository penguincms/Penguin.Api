using Newtonsoft.Json.Linq;
using Penguin.Api.Abstractions.Extensions;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Shared;
using Penguin.Extensions.String;
using Penguin.Json;
using Penguin.Web.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.Json
{
    public class JsonPostItem : BasePostItem<JsonPostPayload, JsonResponsePayload>
    {
        public override IApiServerInteraction<JsonPostPayload, JsonResponsePayload> Execute(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            Container.JavascriptEngine.Execute("globalThis.Playlist = globalThis.Playlist || {};");
            Container.JavascriptEngine.Execute($"globalThis.Playlist['{this.Id}'] = {{}};");

            if (!string.IsNullOrWhiteSpace(this.Request.Body))
            {
                Container.JavascriptEngine.Execute($"globalThis.Playlist['{this.Id}'].Request = {this.Request.Body};");
            }

            IApiServerInteraction<JsonPostPayload, JsonResponsePayload> Interaction = base.Execute(Container);

            if (new JsonString(Interaction.Response.Body).IsValid)
            {
                Container.JavascriptEngine.Execute($"globalThis.Playlist['{this.Id}'].Response = {Interaction.Response.Body};");
            }

            return Interaction;
        }

        public override void FillBody(string source)
        {
            this.Request = this.Request ?? new JsonPostPayload();
            this.Request.Body = source;
        }

        public IEnumerable<JProperty> GetTransformationPoints() => !string.IsNullOrWhiteSpace(this.Request.Body) ? GetTransformationPoints(JObject.Parse(this.Request.Body)) : new List<JProperty>();

        public override JsonPostPayload Transform(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            JsonPostPayload clonedRequest = base.Transform(Container);

            List<JProperty> jprops = this.GetTransformationPoints().ToList();

            foreach (JProperty jprop in jprops)
            {
                if (jprop.Value is JArray ja)
                {
                    this.TransformArray(Container, jprop, ja, clonedRequest);
                }
                else
                {
                    this.TransformValue(Container, jprop, clonedRequest);
                }
            }

            return clonedRequest;
        }

        public void TransformArray(IApiPlaylistSessionContainer Container, JProperty jprop, JArray jarray, JsonPostPayload clonedRequest)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            if (jprop is null)
            {
                throw new ArgumentNullException(nameof(jprop));
            }

            if (jarray is null)
            {
                throw new ArgumentNullException(nameof(jarray));
            }

            if (clonedRequest is null)
            {
                throw new ArgumentNullException(nameof(clonedRequest));
            }

            JArray newArray = new JArray();

            bool replace = false; ;

            foreach (JToken jt in jarray)
            {
                JToken v = jt;

                if (this.TryGetReplacement(v.ToString(), Container, out object newv))
                {
                    if (newv is null)
                    {
                        v = null;
                    }
                    else
                    {
                        v = newv is JToken jto ? jto : new JValue($"{newv}");
                    }

                    replace = true;
                }

                newArray.Add(v);
            }

            if (replace)
            {
                clonedRequest.SetValue(jprop.Path, newArray.ToString(), jprop.Name.Substring(1));
            }
        }

        public void TransformValue(IApiPlaylistSessionContainer Container, JProperty jprop, JsonPostPayload clonedRequest)
        {
            if (jprop is null)
            {
                throw new ArgumentNullException(nameof(jprop));
            }

            if (clonedRequest is null)
            {
                throw new ArgumentNullException(nameof(clonedRequest));
            }

            string SourceValue = jprop.Value.ToString();
            string DestPath = jprop.Path;

            string destPropName = DestPath.FromLast(".").Substring(1);

            if (this.TryFindReplacement(SourceValue, Container, out object v))
            {
                clonedRequest.SetValue(DestPath, v, destPropName);
            }
        }

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<JsonPostPayload, JsonResponsePayload> item) 
            => this.TryCreate(request, response, "application/json", out item);

        public override bool TryGetReplacement(string toReplace, IApiPlaylistSessionContainer Container, out object v) 
            => base.TryGetReplacement(toReplace, Container, out v) || JsonTransformation.TryGetReplacement(toReplace, Container, out v);

        private static IEnumerable<JProperty> GetTransformationPoints(JToken jtoken)
        {
            if (jtoken.Type == JTokenType.Object)
            {
                foreach (JProperty cjprop in GetTransformationPoints(jtoken as JObject))
                {
                    yield return cjprop;
                }
            }

            if (jtoken.Type == JTokenType.Array)
            {
                if (jtoken.Parent is JProperty jp && jp.Name.StartsWith("$", StringComparison.OrdinalIgnoreCase))
                {
                    yield return jp;
                }
                else
                {
                    foreach (JToken jt in jtoken as JArray)
                    {
                        foreach (JProperty cjprop in GetTransformationPoints(jt))
                        {
                            yield return cjprop;
                        }
                    }
                }
            }
        }

        private static IEnumerable<JProperty> GetTransformationPoints(JObject jobject)
        {
            foreach (JProperty jprop in jobject.Properties())
            {
                if (jprop.Name.StartsWith("$", StringComparison.OrdinalIgnoreCase) && jprop.Value.Type == JTokenType.String)
                {
                    yield return jprop;
                }
                else
                {
                    foreach (JProperty cjprop in GetTransformationPoints(jprop.Value))
                    {
                        yield return cjprop;
                    }
                }
            }
        }
    }
}