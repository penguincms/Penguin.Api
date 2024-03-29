﻿using Loxifi;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Extensions.String;
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

            if (SourcePath?.Contains('.') ?? false)
            {
                Path = SourcePath.From(".");
            }
            else
            {
                _ = destination.TryGetValue(DestinationPath, out object oPath);
                Path = oPath.ToString();
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
                _ = responseToCheck.Value.TryGetValue(Path, out object value);
                destination.SetValue(DestinationPath, value);
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out object newValue)
        {
            return source is null ? throw new System.ArgumentNullException(nameof(source)) : source.TryGetValue(SourcePath, out newValue);
        }
    }
}