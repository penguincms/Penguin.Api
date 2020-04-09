using HtmlAgilityPack;
using Penguin.Api.Abstractions.Interfaces;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{
    public class XPathAttributeTransformation : ITransformation
    {
        public string DestinationPath { get; set; }
        public string SourceAttribute { get; set; }
        public string SourceId { get; set; }
        public string SourcePath { get; set; }

        public void Transform(KeyValuePair<string, IApiServerResponse> responseToCheck, IApiPayload destination)
        {
            if (responseToCheck.Key == SourceId)
            {
                if (TryGetTransformedValue(responseToCheck.Value, out string newValue))
                {
                    destination.SetValue(DestinationPath, newValue);
                }
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out string newValue)
        {
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(source.Body);
            HtmlNode signupFormIdElement = htmlDocument.DocumentNode.SelectSingleNode(SourcePath);
            newValue = signupFormIdElement.GetAttributeValue(SourceAttribute, "");
            return true;
        }
    }
}