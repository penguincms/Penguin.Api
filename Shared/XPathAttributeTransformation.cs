﻿using HtmlAgilityPack;
using Penguin.Api.Abstractions.Interfaces;
using System;
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
            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (responseToCheck.Key == this.SourceId)
            {
                if (this.TryGetTransformedValue(responseToCheck.Value, out object newValue))
                {
                    destination.SetValue(this.DestinationPath, newValue);
                }
            }
        }

        public bool TryGetTransformedValue(IApiServerResponse source, out object newValue)
        {
            if (source is null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(source.Body);
            HtmlNode signupFormIdElement = htmlDocument.DocumentNode.SelectSingleNode(this.SourcePath);
            newValue = signupFormIdElement.GetAttributeValue(this.SourceAttribute, "");
            return true;
        }
    }
}