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
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            string Path;

            if (this.SourcePath?.Contains(".") ?? false)
            {
                Path = this.SourcePath.From(".");
            }
            else
            {
                destination.TryGetValue(this.DestinationPath, out object oPath);
                Path = oPath.ToString();
            }

            string Key;
            if (!string.IsNullOrWhiteSpace(this.SourcePath))
            {
                Key = this.SourcePath.To(".");
            }
            else
            {
                Key = Path.To(".");
                Path = Path.From(".");
            }

            if (responseToCheck.Key == Key)
            {
                responseToCheck.Value.TryGetValue(Path, out object value);
                destination.SetValue(this.DestinationPath, value);
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out object newValue) => source is null ? throw new System.ArgumentNullException(nameof(source)) : source.TryGetValue(this.SourcePath, out newValue);
    }
}