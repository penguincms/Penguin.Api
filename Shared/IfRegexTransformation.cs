using Penguin.Api.Abstractions.Interfaces;
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
            if (responseToCheck.Key == SourceId)
            {
                if (this.TryGetTransformedValue(responseToCheck.Value, out string newValue))
                {
                    destination.SetValue(DestinationPath, newValue);
                }
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out string newValue)
        {
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