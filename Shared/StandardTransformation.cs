using Penguin.Api.Abstractions.Interfaces;
using Penguin.Extensions.Strings;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{
    public class StandardTransformation : ITransformation
    {
        public string DestinationPath { get; set; }
        public string SourcePath { get; set; }

        public void Transform(KeyValuePair<string, IApiServerResponse> responseToCheck, IApiPayload destination)
        {
            if(destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            string Path;

            if (SourcePath?.Contains(".") ?? false)
            {
                Path = SourcePath.From(".");
            }
            else
            {
                destination.TryGetValue(DestinationPath, out Path);
            }

            string Key;
            if (!string.IsNullOrWhiteSpace(SourcePath))
            {
                Key = SourcePath.To(".");
            }
            else
            {
                Key = Path.To(".");
                Path = Path.From(".");
            }

            if (responseToCheck.Key == Key)
            {
                responseToCheck.Value.TryGetValue(Path, out string value);
                destination.SetValue(DestinationPath, value);
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out string newValue)
        {
            if (source is null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            return source.TryGetValue(SourcePath, out newValue);
        }
    }
}