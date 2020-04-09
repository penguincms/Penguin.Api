using HtmlAgilityPack;
using Penguin.Api.Abstractions.Interfaces;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{
    public class AppendFormDefaultsTransformation : ITransformation
    {
        string ITransformation.DestinationPath { get; set; }
        public string FormName { get; set; }
        public string SourceId { get; set; }
        string ITransformation.SourcePath { get; set; }

        public void Transform(KeyValuePair<string, IApiServerResponse> responseToCheck, IApiPayload destination)
        {
            if (responseToCheck.Key == this.SourceId)
            {
                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(responseToCheck.Value.Body);

                HtmlAgilityPack.HtmlNode node = htmlDocument.DocumentNode.SelectSingleNode($"//form[@name='{this.FormName}']");

                foreach (HtmlNode cnode in node.SelectNodes(".//input[@name]"))
                {
                    HtmlAttribute valueAttribute = cnode.Attributes["value"];
                    HtmlAttribute typeAttribute = cnode.Attributes["type"];

                    if (typeAttribute != null && typeAttribute.Value == "submit")
                    {
                        continue;
                    }

                    if (valueAttribute != null)
                    {
                        string name = cnode.Attributes["name"].Value;

                        if (!destination.TryGetValue(name, out string _))
                        {
                            destination.SetValue(name, valueAttribute.Value);
                        }
                    }
                }
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out string newValue)
        {
            newValue = null;
            return false;
        }
    }
}