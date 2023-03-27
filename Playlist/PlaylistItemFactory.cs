using Loxifi;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Shared;
using Penguin.Reflection;
using Penguin.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.Playlist
{
    public static class PlaylistItemFactory
    {
        private static List<IHttpPlaylistItem> playlistTemplates;

        public static IEnumerable<IHttpPlaylistItem> PlaylistTemplates
        {
            get
            {
                playlistTemplates ??= TypeFactory.Default.GetAllImplementations<IHttpPlaylistItem>().Select(t => Activator.CreateInstance(t) as IHttpPlaylistItem).ToList();

                return playlistTemplates;
            }
        }

        public static IPlaylistItem GetPlaylistItem(string Name, HttpServerInteraction interaction)
        {
            return interaction is null
                ? throw new ArgumentNullException(nameof(interaction))
                : GetPlaylistItem(Name, interaction.Request, interaction.Response);
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

            IHttpPlaylistItem item = null;

            foreach (IHttpPlaylistItem checkItem in PlaylistTemplates)
            {
                if (checkItem.TryCreate(request, response, out item))
                {
                    break;
                }
            }

            item ??= new UnsupportedHttpPlaylistItem(request, response);

            item.Id = Name;

            return item;
        }

        public static IEnumerable<IPlaylistItem> GetPlaylistItems(HttpServerCapture capture)
        {
            if (capture is null)
            {
                throw new ArgumentNullException(nameof(capture));
            }

            for (int i = 0; i < capture.Count; i++)
            {
                HttpServerInteraction interaction = capture[i];

                string interactionName = $"{i:0000}";

                yield return GetPlaylistItem(interactionName, interaction);
            }
        }
    }
}