using Penguin.Api.Abstractions.Interfaces;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Forms
{
    public class FormTransformation : ITransformation
    {
        public string DestinationPath { get; set; }
        public string SourceId { get; set; }
        public string SourcePath { get; set; }

        public void Transform(KeyValuePair<string, IApiServerResponse> responseToCheck, IApiPayload destination)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (responseToCheck.Value is null)
            {
                throw new ArgumentNullException(nameof(responseToCheck));
            }

            if (responseToCheck.Key != this.SourceId)
            {
                return;
            }

            responseToCheck.Value.TryGetValue(this.SourcePath, out object sourceValue);

            if (!string.IsNullOrWhiteSpace(this.DestinationPath))
            {
                destination.SetValue(this.DestinationPath, sourceValue);
            }
            else
            {
                destination.SetValue(this.SourcePath, sourceValue);
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out object newValue) => source is null ? throw new ArgumentNullException(nameof(source)) : source.TryGetValue(this.SourcePath, out newValue);
    }
}