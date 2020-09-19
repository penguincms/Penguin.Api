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
        public int Count => ((IList<IPlaylistItem>)this.Items).Count;
        public bool IsReadOnly => ((IList<IPlaylistItem>)this.Items).IsReadOnly;
        public Action<IApiServerInteraction> OnResponse { get; set; } = null;
        protected List<IPlaylistItem> Items { get; set; } = new List<IPlaylistItem>();
        public IPlaylistItem this[int index] { get => ((IList<IPlaylistItem>)this.Items)[index]; set => ((IList<IPlaylistItem>)this.Items)[index] = value; }

        public void Add(IPlaylistItem item)
        {
            if (this.Items.Any(i => i.Id == item.Id))
            {
                throw new ArgumentException("Can not add item with duplicate Id");
            }

            ((IList<IPlaylistItem>)this.Items).Add(item);
        }

        public void Clear() => ((IList<IPlaylistItem>)this.Items).Clear();

        public bool Contains(IPlaylistItem item) => ((IList<IPlaylistItem>)this.Items).Contains(item);

        public bool Contains(string id) => this.Items.Any(i => i.Id == id);

        public void CopyTo(IPlaylistItem[] array, int arrayIndex) => ((IList<IPlaylistItem>)this.Items).CopyTo(array, arrayIndex);

        public ApiPlaylistSessionContainer Execute(PlaylistSettings playlistSettings, PlaylistExecutionSettings executionSettings) => this.Execute(playlistSettings, null, executionSettings);

        public ApiPlaylistSessionContainer Execute(PlaylistSettings playlistSettings, bool firstRun) => this.Execute(playlistSettings, null, new PlaylistExecutionSettings(firstRun));

        public ApiPlaylistSessionContainer Execute(PlaylistSettings playlistSettings, ApiPlaylistSessionContainer Container = null, PlaylistExecutionSettings executionSettings = null)
        {
            if (playlistSettings is null)
            {
                throw new ArgumentNullException(nameof(playlistSettings));
            }

            executionSettings = executionSettings ?? new PlaylistExecutionSettings();

            Container = Container ?? new ApiPlaylistSessionContainer()
            {
                DisposeAfterUse = true
            };

            ConfigurationResponseWrapper configurationResponseWrapper = new ConfigurationResponseWrapper();

            if (executionSettings.ExecuteCustomJavascript)
            {
                Container.JavascriptEngine.Execute(playlistSettings.CustomJavascript);
            }

            if (executionSettings.CopyConfigurations)
            {
                StringBuilder JsConfigScript = new StringBuilder();
                JsConfigScript.AppendLine("globalThis.$ = globalThis.$ || {};");

                foreach (PlaylistConfiguration configuration in playlistSettings.Configurations)
                {
                    configurationResponseWrapper.Add(configuration.Key, configuration.Value);
                    JsConfigScript.AppendLine($"globalThis.$.{configuration.Key} = {JsonConvert.SerializeObject(configuration.Value)};");
                }

                Container.JavascriptEngine.Execute(JsConfigScript.ToString());

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

                    this.OnResponse?.Invoke(Value);

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

        public IPlaylistItem Find(string id) => this.Items.FirstOrDefault(i => i.Id == id);

        public IEnumerator<IPlaylistItem> GetEnumerator() => ((IList<IPlaylistItem>)this.Items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IList<IPlaylistItem>)this.Items).GetEnumerator();

        public int IndexOf(IPlaylistItem item) => ((IList<IPlaylistItem>)this.Items).IndexOf(item);

        public int IndexOf(string id) => this.TryFind(id, out IPlaylistItem result) ? ((IList<IPlaylistItem>)this.Items).IndexOf(result) : -1;

        public void Insert(int index, IPlaylistItem item) => ((IList<IPlaylistItem>)this.Items).Insert(index, item);

        public bool Remove(string id) => this.TryFind(id, out IPlaylistItem result) && this.Remove(result);

        public bool Remove(IPlaylistItem item) => ((IList<IPlaylistItem>)this.Items).Remove(item);

        public void RemoveAt(int index) => ((IList<IPlaylistItem>)this.Items).RemoveAt(index);

        public void Reset()
        {
            foreach (IPlaylistItem item in this.Items)
            {
                item.Reset();
            }
        }

        public bool TryFind(string id, out IPlaylistItem result)
        {
            result = this.Find(id);

            return !(result is null);
        }
    }
}