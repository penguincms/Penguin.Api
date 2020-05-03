using Penguin.Api.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Penguin.Api.Shared
{
    public class RegexTransformation : ITransformation
    {
        public string DestinationPath { get; set; }
        public string RegexExpression { get; set; }
        public string SourceId { get; set; }
        string ITransformation.SourcePath { get; set; }
        public string Value { get; set; }
        public int Group { get; set; }
        public int MatchIndex { get; set; }

        public void Transform(KeyValuePair<string, IApiServerResponse> responseToCheck, IApiPayload destination)
        {
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

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
            newValue = null;

            if (source is null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            if (Regex.IsMatch(source.Body, RegexExpression))
            {
                int mIndex = 0;
                foreach (Match m in Regex.Matches(source.Body, RegexExpression))
                {
                    if (MatchIndex == mIndex)
                    {
                        int gIndex = 0;

                        foreach (Group g in m.Groups)
                        {
                            if (gIndex == Group)
                            {
                                newValue = g.Value;
                                return true;
                            }
                            gIndex++;

                            if(gIndex > Group)
                            {
                                return false;
                            }
                        }
                    }

                    mIndex++;

                    if(mIndex > MatchIndex)
                    {
                        return false;
                    }
                }
            }

            return false;

        }
    }
}