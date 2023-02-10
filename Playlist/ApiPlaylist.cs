using Newtonsoft.Json;
using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.SystemItems;
using Penguin.Web.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Penguin.Api.Playlist
{
    public class ApiPlaylist : IList<IPlaylistItem>
    {
        public int Count => ((IList<IPlaylistItem>)Items).Count;
        public bool IsReadOnly => ((IList<IPlaylistItem>)Items).IsReadOnly;
        public Action<IApiServerInteraction> OnResponse { get; set; }
        protected List<IPlaylistItem> Items { get; set; } = new List<IPlaylistItem>();
        public IPlaylistItem this[int index] { get => ((IList<IPlaylistItem>)Items)[index]; set => ((IList<IPlaylistItem>)Items)[index] = value; }

        public void Add(IPlaylistItem item)
        {
            if (Items.Any(i => i.Id == item.Id))
            {
                throw new ArgumentException("Can not add item with duplicate Id");
            }

            ((IList<IPlaylistItem>)Items).Add(item);
        }

        public void Clear()
        {
            ((IList<IPlaylistItem>)Items).Clear();
        }

        public bool Contains(IPlaylistItem item)
        {
            return ((IList<IPlaylistItem>)Items).Contains(item);
        }

        public bool Contains(string id)
        {
            return Items.Any(i => i.Id == id);
        }

        public void CopyTo(IPlaylistItem[] array, int arrayIndex)
        {
            ((IList<IPlaylistItem>)Items).CopyTo(array, arrayIndex);
        }

        public ApiPlaylistSessionContainer Execute(PlaylistSettings playlistSettings, PlaylistExecutionSettings executionSettings)
        {
            return Execute(playlistSettings, null, executionSettings);
        }

        public ApiPlaylistSessionContainer Execute(PlaylistSettings playlistSettings, bool firstRun)
        {
            return Execute(playlistSettings, null, new PlaylistExecutionSettings(firstRun));
        }

        public ApiPlaylistSessionContainer Execute(PlaylistSettings playlistSettings, ApiPlaylistSessionContainer Container = null, PlaylistExecutionSettings executionSettings = null)
        {
            if (playlistSettings is null)
            {
                throw new ArgumentNullException(nameof(playlistSettings));
            }

            executionSettings ??= new PlaylistExecutionSettings();

            Container ??= new ApiPlaylistSessionContainer()
            {
                DisposeAfterUse = true
            };

            ConfigurationResponseWrapper configurationResponseWrapper = new();

            if (executionSettings.ExecuteCustomJavascript)
            {
                _ = Container.JavascriptEngine.Execute(playlistSettings.CustomJavascript);
            }

            if (executionSettings.CopyConfigurations)
            {
                StringBuilder JsConfigScript = new();
                _ = JsConfigScript.AppendLine("globalThis.$ = globalThis.$ || {};");

                foreach (PlaylistConfiguration configuration in playlistSettings.Configurations)
                {
                    configurationResponseWrapper.Add(configuration.Key, configuration.Value);
                    _ = JsConfigScript.AppendLine($"globalThis.$.{configuration.Key} = {JsonConvert.SerializeObject(configuration.Value)};");
                }

                _ = Container.JavascriptEngine.Execute(JsConfigScript.ToString());

                Container.Interactions.Add("$", configurationResponseWrapper);
            }

            if (executionSettings.CopyHeaders)
            {
                foreach (HttpHeader header in playlistSettings.Headers)
                {
                    Container.Client.Headers.Add(header.Key, header.Value);
                }
            }

            bool Continue = executionSettings.StartId != null;

            foreach (IPlaylistItem item in this)
            {
                if (!item.Enabled)
                {
                    continue;
                }

                if (Continue)
                {
                    if (item.Id == executionSettings.StartId)
                    {
                        Continue = false;
                    }
                    else
                    {
                        continue;
                    }
                }

                if ((item.Conditions?.Any() ?? false) && !item.Conditions.All(c => c.ShouldExecute(Container)))
                {
                    continue;
                }

                string Key = item.Id;

                if (item is IHttpPlaylistItem hpi)
                {
                    IApiServerInteraction Value = hpi.Execute(Container);

                    OnResponse?.Invoke(Value);

                    Container.Interactions.Add(Value);

                    if (Value.Response.Status == ApiServerResponseStatus.Error)
                    {
                        break;
                    }
                }
                else
                {
                    item.Execute(Container);
                }

                if (!string.IsNullOrWhiteSpace(executionSettings.StopId) && Key == executionSettings.StopId)
                {
                    break;
                }
            }

            return Container;
        }

        public IPlaylistItem Find(string id)
        {
            return Items.FirstOrDefault(i => i.Id == id);
        }

        public IEnumerator<IPlaylistItem> GetEnumerator()
        {
            return ((IList<IPlaylistItem>)Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<IPlaylistItem>)Items).GetEnumerator();
        }

        public int IndexOf(IPlaylistItem item)
        {
            return ((IList<IPlaylistItem>)Items).IndexOf(item);
        }

        public int IndexOf(string id)
        {
            return TryFind(id, out IPlaylistItem result) ? ((IList<IPlaylistItem>)Items).IndexOf(result) : -1;
        }

        public void Insert(int index, IPlaylistItem item)
        {
            ((IList<IPlaylistItem>)Items).Insert(index, item);
        }

        public bool Remove(string id)
        {
            return TryFind(id, out IPlaylistItem result) && Remove(result);
        }

        public bool Remove(IPlaylistItem item)
        {
            return ((IList<IPlaylistItem>)Items).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<IPlaylistItem>)Items).RemoveAt(index);
        }

        public void Reset()
        {
            foreach (IPlaylistItem item in Items)
            {
                item.Reset();
            }
        }

        public bool TryFind(string id, out IPlaylistItem result)
        {
            result = Find(id);

            return result is not null;
        }
    }
}