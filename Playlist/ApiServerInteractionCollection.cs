using Penguin.Api.Abstractions.Interfaces;
using Penguin.SystemExtensions.Abstractions.Interfaces;
using Penguin.SystemExtensions.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.Playlist
{
    public class ApiServerInteractionCollection : IApiServerInteractionCollection
    {
        private readonly Dictionary<string, IApiServerInteraction> Interactions = new Dictionary<string, IApiServerInteraction>();

        public IPropertyDictionary<string, IApiPayload> Requests { get; protected set; }

        public IPropertyDictionary<string, IApiServerResponse> Responses { get; protected set; }

        public ApiServerInteractionCollection()
        {
            this.Requests = new PropertyDictionary<string, IApiServerInteraction, IApiPayload>(this.Interactions, (i) => i.Request);
            this.Responses = new PropertyDictionary<string, IApiServerInteraction, IApiServerResponse>(this.Interactions, (i) => i.Response);
        }

        public IApiServerInteraction this[string id] => this.Interactions[id];

        public void Add(string id, IApiServerResponse response)
        {
            ApiServerInteraction interaction = this.FindOrCreate(id);

            if (interaction.Response != null)
            {
                throw new ArgumentException($"Api Interaction with id {id} has existing response");
            }

            interaction.Response = response;
        }

        public void Add(string id, IApiPayload request)
        {
            ApiServerInteraction interaction = this.FindOrCreate(id);

            if (interaction.Request != null)
            {
                throw new ArgumentException($"Api Interaction with id {id} has existing request");
            }

            interaction.Request = request;
        }

        public void Add(string id, IApiServerInteraction interaction)
        {
            if (this.Interactions.ContainsKey(id))
            {
                throw new ArgumentException($"Api Interaction with id {id} already exists");
            }
            else
            {
                this.Interactions.Add(id, interaction);
            }
        }

        public void Add(IApiServerInteraction interaction)
        {
            if (interaction is null)
            {
                throw new ArgumentNullException(nameof(interaction));
            }

            this.Add(interaction.Id, interaction);
        }

        public IEnumerator<IApiServerInteraction> GetEnumerator()
        {
            foreach (KeyValuePair<string, IApiServerInteraction> kvp in this.Interactions)
            {
                yield return kvp.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public (string Id, IApiPayload Request) Request(string id) => (id, this.Interactions.Single(k => k.Key == id).Value.Request);

        public (string Id, IApiServerResponse Response) Response(string id) => (id, this.Interactions.Single(k => k.Key == id).Value.Response);

        private ApiServerInteraction FindOrCreate(string id)
        {
            if (!this.Interactions.TryGetValue(id, out IApiServerInteraction apiServerInteraction))
            {
                apiServerInteraction = new ApiServerInteraction();

                this.Interactions.Add(id, apiServerInteraction);
            }

            return (ApiServerInteraction)apiServerInteraction;
        }
    }
}