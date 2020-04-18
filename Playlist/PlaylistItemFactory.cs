using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Forms;
using Penguin.Api.Json;
using Penguin.Api.Shared;
using Penguin.Api.SystemItems;
using Penguin.Web;
using Penguin.Web.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.Playlist
{
    public static class PlaylistItemFactory
    {
        public static IPlaylistItem GetPlaylistItem(string Name, HttpServerInteraction interaction)
        {
            if (interaction is null)
            {
                throw new ArgumentNullException(nameof(interaction));
            }

            return GetPlaylistItem(Name, interaction.Request, interaction.Response);
        }
        public static IPlaylistItem GetPlaylistItem(string Name, HttpServerRequest request, HttpServerResponse response = null)
        {
            if (Name is null)
            {
                throw new ArgumentNullException(nameof(Name));
            }

            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            bool isJson = false;
            bool isForm = false;
            bool isText = false;
            bool isBinary = false;
            bool isEmpty = false;

            string contentType = request.Headers["Content-Type"] ?? string.Empty;

            if (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                isJson = true;
            }
            else if (contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                isForm = true;
            }
            else if (contentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase))
            {
                isText = true;
            }
            else if (string.IsNullOrEmpty(contentType))
            {
                isEmpty = true;
            }
            else
            {
                isBinary = true;
            }

            IPlaylistItem toReturn;

            List<PostMethod> postMethods = new List<PostMethod>();

            foreach (PostMethod m in Enum.GetValues(typeof(PostMethod))) {
                postMethods.Add(m);
            }


            if (postMethods.Any(m => m.ToString() == request.Method) && isEmpty)
            {
                toReturn = new EmptyPostItem();
            } else if (postMethods.Any(m => m.ToString() == request.Method) && isJson)
            {
                toReturn = new JsonPostItem();
            }
            else if (request.Method == "GET" && !isJson)
            {
                toReturn = new HttpGetItem();
            }
            else if (postMethods.Any(m => m.ToString() == request.Method) && isForm)
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
            else if (postMethods.Any(m => m.ToString() == request.Method) && isText)
            {
                toReturn = new TextPostItem();
            }
            else
            {
                toReturn = new UnsupportedPlaylistItem(request, response);
            }

            toReturn.Id = Name;

            if (toReturn is IPostItem postItem)
            {
                postItem.FillBody(request.BodyText);

                postItem.Method = postMethods.Single(m => m.ToString() == request.Method);
            }

            foreach (HttpHeader header in request.Headers)
            {
                toReturn.Request.Headers.Add(header.Key, header.Value);
            }

            if (response != null)
            {
                foreach (HttpHeader header in request.Headers)
                {
                    toReturn.Response.Headers.Add(header.Key, header.Value);
                }

                toReturn.Response.Body = response.BodyText;
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

                yield return GetPlaylistItem(interactionName, interaction);
            }
        }
    }
}