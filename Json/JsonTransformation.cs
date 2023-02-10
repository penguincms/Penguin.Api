using Newtonsoft.Json.Linq;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Extensions.String;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Json
{
    public class JsonTransformation : ITransformation
    {
        public string DestinationPath { get; set; }

        public string SourceId { get; set; }

        public string SourcePath { get; set; }

        public static bool TryGetReplacement(string toReplace, IApiPlaylistSessionContainer Container, out object newValue)
        {
            if (toReplace is null)
            {
                throw new ArgumentNullException(nameof(toReplace));
            }

            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            if (toReplace.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
            {
                string toExecute = toReplace.From(":");

                toExecute = $"function toExec() {{ var v = {toExecute}; return v.toString(); }} toExec();";

                newValue = Container.JavascriptEngine.Execute(toExecute);
                return true;
            }
            else
            {
                (string sourceId, string sourcePath) = SplitPath(toReplace);

                if (Container.Interactions.Responses.TryGetValue(sourceId, out IApiServerResponse response))
                {
                    return TryGetTransformedValue(response, sourcePath, out newValue);
                }
                else
                {
                    newValue = null;
                    return false;
                }
            }
        }

        public static (string Id, string Path) SplitPath(string value)
        {
            return (value.To("."), value.From("."));
        }

        public static bool TryGetTransformedValue(IApiServerResponse payload, string sourcePath, out object newValue)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.Body is null)
            {
                newValue = null;
                return false;
            }

            JToken sourceObject = JToken.Parse(payload.Body);

            JToken sourceToken = sourceObject.SelectToken(sourcePath);

            newValue = sourceToken;

            return true;
        }

        public void Transform(KeyValuePair<string, IApiServerResponse> responseToCheck, IApiPayload destination)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (responseToCheck.Key == SourceId)
            {
                if (responseToCheck.Value.TryGetValue(SourcePath, out object value))
                {
                    destination.SetValue(DestinationPath, value);
                }
            }
        }

        bool ITransformation.TryGetTransformedValue(IApiServerResponse payload, out object newValue)
        {
            return TryGetTransformedValue(payload, SourcePath, out newValue);
        }
    }
}