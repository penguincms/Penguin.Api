using Penguin.Api.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Penguin.Api.Shared
{
    public class IfRegexTransformation : ITransformation
    {
        public string DestinationPath { get; set; }
        public string RegexExpression { get; set; }
        public string SourceId { get; set; }
        string ITransformation.SourcePath { get; set; }
        public string Value { get; set; }

        public void Transform(KeyValuePair<string, IApiServerResponse> responseToCheck, IApiPayload destination)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (responseToCheck.Key == SourceId)
            {
                if (TryGetTransformedValue(responseToCheck.Value, out object newValue))
                {
                    destination.SetValue(DestinationPath, newValue);
                }
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out object newValue)
        {
            if (source is null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            if (Regex.IsMatch(source.Body, RegexExpression))
            {
                newValue = Value;
                return true;
            }
            else
            {
                newValue = null;
                return false;
            }
        }
    }
}