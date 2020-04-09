using Newtonsoft.Json.Linq;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Shared;
using Penguin.Extensions.Strings;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Json
{
    public class JsonPostItem : ApiServerPost<JsonPostPayload, JsonResponsePayload>
    {
        public override JsonResponsePayload Execute(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            Container.JavascriptEngine.Execute("var Playlist = Playlist || {};");
            Container.JavascriptEngine.Execute($"Playlist['{Id}'] = {{}};");

            if (!string.IsNullOrWhiteSpace(this.Request.Body))
            {
                Container.JavascriptEngine.Execute($"Playlist['{Id}'].Request = {this.Request.Body};");
            }

            JsonResponsePayload Response = base.Execute(Container);

            if (!string.IsNullOrWhiteSpace(Response.Body))
            {
                Container.JavascriptEngine.Execute($"Playlist['{Id}'].Response = {Response.Body};");
            }

            return Response;
        }

        public override void FillBody(string source)
        {
            this.Request = this.Request ?? new JsonPostPayload();
            this.Request.Body = source;
        }

        public override string GetReplacement(string toReplace, IApiPlaylistSessionContainer Container) => JsonTransformation.GetReplacement(toReplace, Container);

        public IEnumerable<JProperty> GetTransformationPoints() => !string.IsNullOrWhiteSpace(this.Request.Body) ? GetTransformationPoints(JObject.Parse(this.Request.Body)) : new List<JProperty>();

        public override JsonPostPayload Transform(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            JsonPostPayload clonedRequest = base.Transform(Container);

            foreach (JProperty jprop in GetTransformationPoints())
            {
                string SourceValue = jprop.Value.ToString();
                string DestPath = jprop.Path;

                string destPropName = DestPath.FromLast(".").Substring(1);

                if (SourceValue.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                {
                    //JProperty destProperty = toPostObj.SelectToken(DestPath).Parent as JProperty;

                    string newValue = GetReplacement(SourceValue, Container);

                    clonedRequest.SetValue(DestPath, newValue, destPropName);

                    //if (newValue.StartsWith("[", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    propParent.Add(jprop.Name.Substring(1), JArray.Parse(newValue));
                    //}
                    //else if (newValue.StartsWith("{", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    propParent.Add(jprop.Name.Substring(1), JObject.Parse(newValue));
                    //}
                    //else
                    //{
                    //    propParent.Add(jprop.Name.Substring(1), new JValue(newValue));
                    //}

                    //toPost = toPostObj.ToString();
                }
                else
                {
                    clonedRequest.SetValue(DestPath, base.GetReplacement(SourceValue, Container), destPropName);
                }
            }

            return clonedRequest;
        }

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
                foreach (JToken jt in jtoken as JArray)
                {
                    foreach (JProperty cjprop in GetTransformationPoints(jt))
                    {
                        yield return cjprop;
                    }
                }
            }
        }

        private static IEnumerable<JProperty> GetTransformationPoints(JObject jobject)
        {
            foreach (JProperty jprop in jobject.Properties())
            {
                if (jprop.Name.StartsWith("$") && jprop.Value.Type == JTokenType.String)
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