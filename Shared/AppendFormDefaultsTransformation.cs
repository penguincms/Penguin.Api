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
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (responseToCheck.Key == SourceId)
            {
                HtmlAgilityPack.HtmlDocument htmlDocument = new();
                htmlDocument.LoadHtml(responseToCheck.Value.Body);

                HtmlNode node;
                if (FormName.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    node = htmlDocument.DocumentNode.SelectSingleNode($"//form[@id='{FormName[1..]}']");

                    if (node is null)
                    {
                        throw new NullReferenceException($"Form with id \"{FormName[1..]}\" was not found on response \"{SourceId}\"");
                    }
                }
                else
                {
                    node = htmlDocument.DocumentNode.SelectSingleNode($"//form[@name='{FormName}']");

                    if (node is null)
                    {
                        throw new NullReferenceException($"Form with name \"{FormName}\" was not found on response \"{SourceId}\"");
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

                        if (!destination.TryGetValue(name, out _))
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

        public bool TryGetTransformedValue(IApiServerResponse source, out object newValue)
        {
            newValue = null;
            return false;
        }
    }
}