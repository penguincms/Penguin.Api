using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Forms;
using Penguin.Api.Json;
using Penguin.Api.Shared;
using Penguin.Api.SystemItems;
using Penguin.Web;
using Penguin.Web.Headers;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Playlist
{
    public static class PlaylistItemFactory
    {
        public static IPlaylistItem GetPlaylistItem(string Name, HttpServerRequest request)
        {
            bool isJson = false;
            bool isForm = false;
            bool isText = false;
            bool isBinary = false;

            string contentType = request.Headers["Content-Type"] ?? string.Empty;

            if (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                isJson = true;
            }
            else if (contentType.StartsWith("application/x-www-form-urlencoded"))
            {
                isForm = true;
            }
            else if (contentType.StartsWith("text/plain"))
            {
                isText = true;
            }
            else
            {
                isBinary = true;
            }

            IPlaylistItem toReturn;

            if (request.Method == "POST" && isJson)
            {
                toReturn = new JsonPostItem();
            }
            else if (request.Method == "GET" && !isJson)
            {
                toReturn = new HttpGetItem();
            }
            else if (request.Method == "POST" && isForm)
            {
                toReturn = new FormPostItem();
            }
            else if (request.Method == "GET" && isJson)
            {
                toReturn = new JsonGetItem();
            }
            else if (request.Method == "CONNECT")
            {
                toReturn = new ConnectItem();
            }
            else if (request.Method == "POST" && isText)
            {
                toReturn = new TextPostItem();
            }
            else
            {
                throw new ArgumentException($"Unsupported playlist configuration (Method: {request.Method}, Content-Type: {contentType})");
            }

            toReturn.Id = Name;

            if (toReturn is IPostItem postItem)
            {
                postItem.FillBody(request.BodyText);
            }

            foreach (HttpHeader header in request.Headers)
            {
                toReturn.Request.Headers.Add(header.Key, header.Value);
            }

            toReturn.Url = request.Url;

            return toReturn;
        }

        public static IEnumerable<IPlaylistItem> GetPlaylistItems(HttpServerCapture capture)
        {
            for (int i = 0; i < capture.Count; i++)
            {
                HttpServerInteraction interaction = capture[i];

                string interactionName = $"{i:0000}";

                yield return GetPlaylistItem(interactionName, interaction.Request);
            }
        }
    }
}