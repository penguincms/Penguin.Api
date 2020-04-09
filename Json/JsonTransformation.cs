using Newtonsoft.Json.Linq;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Extensions.Strings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.Json
{
    public class JsonTransformation : ITransformation
    {
        public string DestinationPath { get; set; }
        public string SourceId { get; set; }
        public string SourcePath { get; set; }

        public static string GetReplacement(string toReplace, IApiPlaylistSessionContainer Container)
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

                return Container.JavascriptEngine.Execute(toExecute);
            }
            else
            {
                string sourceId = toReplace.To(".");

                string sourcePath = toReplace.From(".");

                if (TryGetTransformedValue(Container.PreviousResponses.Single(v => v.Key == sourceId).Value, sourcePath, out string newValue))
                {
                    return newValue;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool TryGetTransformedValue(IApiServerResponse payload, string sourcePath, out string newValue)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            JToken sourceObject = JToken.Parse(payload.Body);

            JToken sourceToken = sourceObject.SelectToken(sourcePath);

            newValue = sourceToken?.ToString();

            return true;
        }

        public void Transform(KeyValuePair<string, IApiServerResponse> responseToCheck, IApiPayload destination)
        {
            if (responseToCheck.Key == SourceId)
            {
                if (responseToCheck.Value.TryGetValue(SourcePath, out string value))
                {
                    destination.SetValue(DestinationPath, value);
                }
            }
        }

        bool ITransformation.TryGetTransformedValue(IApiServerResponse payload, out string newValue) => TryGetTransformedValue(payload, SourcePath, out newValue);
    }
}