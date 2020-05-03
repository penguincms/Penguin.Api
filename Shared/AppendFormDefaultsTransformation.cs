using HtmlAgilityPack;
using Penguin.Api.Abstractions.Interfaces;
using System;
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
            if(destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (responseToCheck.Key == this.SourceId)
            {
                HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                htmlDocument.LoadHtml(responseToCheck.Value.Body);

                HtmlAgilityPack.HtmlNode node = null;

                if (this.FormName.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    node = htmlDocument.DocumentNode.SelectSingleNode($"//form[@id='{this.FormName.Substring(1)}']");

                    if(node is null)
                    {
                        throw new NullReferenceException($"Form with id \"{this.FormName.Substring(1)}\" was not found on response \"{this.SourceId}\"");
                    }
                } else
                {
                    node = htmlDocument.DocumentNode.SelectSingleNode($"//form[@name='{this.FormName}']");

                    if (node is null)
                    {
                        throw new NullReferenceException($"Form with name \"{this.FormName}\" was not found on response \"{this.SourceId}\"");
                    }
                }

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

            if (destination is null)
            {
                throw new System.ArgumentNullException(nameof(destination));
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out string newValue)
        {
            newValue = null;
            return false;
        }
    }
}