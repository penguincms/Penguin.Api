using Penguin.Web.Headers;
using System.Collections.Generic;

namespace MassageEnvy.Meevo.Playback
{
    public class PlaylistConfiguration
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class PlaylistSettings
    {
        public List<PlaylistConfiguration> Configurations { get; set; } = new List<PlaylistConfiguration>();
        public string CustomJavascript { get; set; }
        public List<HttpHeader> Headers { get; set; } = new List<HttpHeader>();
        public string StopAfterId { get; set; }
    }
}