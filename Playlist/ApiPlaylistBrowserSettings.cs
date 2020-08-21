using Penguin.Json.Extensions;
using Penguin.Web.Headers;
using System.Collections.Generic;
using System.Reflection;

namespace Penguin.Api.Playlist
{
    public class PlaylistConfiguration
    {
        public virtual string Key { get; set; }
        public virtual object Value { get; set; }
    }

    public class PlaylistSettings
    {
        public List<PlaylistConfiguration> Configurations { get; set; } = new List<PlaylistConfiguration>();

        public string CustomJavascript { get; set; }

        public List<HttpHeader> Headers { get; set; } = new List<HttpHeader>();

        public void AddConfiguration(object o)
        {
            if (o is null)
            {
                throw new System.ArgumentNullException(nameof(o));
            }

            foreach (PropertyInfo pi in o.GetType().GetProperties())
            {
                if (pi.GetGetMethod() is null)
                {
                    continue;
                }

                string jsonName = pi.GetJsonName();
                string value = pi.GetValue(o)?.ToString();

                this.Configurations.Add(new PlaylistConfiguration()
                {
                    Key = jsonName,
                    Value = value
                });
            }
        }
    }
}