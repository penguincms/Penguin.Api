using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.SystemItems;
using Penguin.Web.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MassageEnvy.Meevo.Playback
{
    public class ApiPlaylist : IList<IPlaylistItem>
    {
        public Action<IApiServerInteraction> OnResponse = null;
        public int Count => ((IList<IPlaylistItem>)this.Items).Count;
        public bool IsReadOnly => ((IList<IPlaylistItem>)this.Items).IsReadOnly;
        protected List<IPlaylistItem> Items { get; set; } = new List<IPlaylistItem>();
        public IPlaylistItem this[int index] { get => ((IList<IPlaylistItem>)this.Items)[index]; set => ((IList<IPlaylistItem>)this.Items)[index] = value; }

        public void Reset()
        {
            foreach(IPlaylistItem item in Items)
            {
                item.Reset();
            }
        }

        public void Add(IPlaylistItem item)
        {
            if (Items.Any(i => i.Id == item.Id))
            {
                throw new ArgumentException("Can not add item with duplicate Id");
            }

            ((IList<IPlaylistItem>)this.Items).Add(item);
        }

        public void Clear()
        {
            ((IList<IPlaylistItem>)this.Items).Clear();
        }

        public bool Contains(IPlaylistItem item)
        {
            return ((IList<IPlaylistItem>)this.Items).Contains(item);
        }

        public bool Contains(string id)
        {
            return Items.Any(i => i.Id == id);
        }

        public void CopyTo(IPlaylistItem[] array, int arrayIndex)
        {
            ((IList<IPlaylistItem>)this.Items).CopyTo(array, arrayIndex);
        }

        public ApiPlaylistSessionContainer Execute(PlaylistSettings playlistSettings, ApiPlaylistSessionContainer Container = null)
        {
            if (playlistSettings is null)
            {
                throw new ArgumentNullException(nameof(playlistSettings));
            }

            Container = Container ?? new ApiPlaylistSessionContainer()
            {
                DisposeAfterUse = true
            };

            ConfigurationResponseWrapper configurationResponseWrapper = new ConfigurationResponseWrapper();

            Container.JavascriptEngine.Execute(playlistSettings.CustomJavascript);

            foreach (PlaylistConfiguration configuration in playlistSettings.Configurations)
            {
                configurationResponseWrapper.Add(configuration.Key, configuration.Value);
            }

            Container.Interactions.Add("$", configurationResponseWrapper);

            foreach (HttpHeader header in playlistSettings.Headers)
            {
                Container.Client.Headers.Add(header.Key, header.Value);
            }

            foreach (IPlaylistItem item in this)
            {
                if(!item.Enabled)
                {
                    continue;
                }

                string Key = item.Id;

                IApiServerInteraction Value = item.Execute(Container);

                OnResponse?.Invoke(Value);

                Container.Interactions.Add(Value);

                if ((!string.IsNullOrWhiteSpace(playlistSettings.StopAfterId) && Key == playlistSettings.StopAfterId) || Value.Response.Status == ApiServerResponseStatus.Error)
                {
                    break;
                }
            }

            return Container;
        }

        public IPlaylistItem Find(string id)
        {
            return this.Items.FirstOrDefault(i => i.Id == id);
        }

        public IEnumerator<IPlaylistItem> GetEnumerator()
        {
            return ((IList<IPlaylistItem>)this.Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<IPlaylistItem>)this.Items).GetEnumerator();
        }

        public int IndexOf(IPlaylistItem item)
        {
            return ((IList<IPlaylistItem>)this.Items).IndexOf(item);
        }

        public int IndexOf(string id)
        {
            if (TryFind(id, out IPlaylistItem result))
            {
                return ((IList<IPlaylistItem>)this.Items).IndexOf(result);
            }
            else
            {
                return -1;
            }
        }

        public void Insert(int index, IPlaylistItem item)
        {
            ((IList<IPlaylistItem>)this.Items).Insert(index, item);
        }

        public bool Remove(string id)
        {
            if (TryFind(id, out IPlaylistItem result))
            {
                return this.Remove(result);
            }
            else
            {
                return false;
            }
        }

        public bool Remove(IPlaylistItem item)
        {
            return ((IList<IPlaylistItem>)this.Items).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<IPlaylistItem>)this.Items).RemoveAt(index);
        }

        public bool TryFind(string id, out IPlaylistItem result)
        {
            result = Find(id);

            return !(result is null);
        }
    }
}