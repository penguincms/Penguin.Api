using Penguin.Api.Abstractions.Interfaces;
using Penguin.SystemExtensions.Abstractions.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Penguin.SystemExtensions.Collections;

namespace Penguin.Api.Playlist
{
    public class ApiServerInteractionCollection : IApiServerInteractionCollection
    {
        private Dictionary<string, IApiServerInteraction> Interactions = new Dictionary<string, IApiServerInteraction>();

        public ApiServerInteractionCollection()
        {
            this.Requests = new PropertyDictionary<string, IApiServerInteraction, IApiPayload>(Interactions, (i) => i.Request);
            this.Responses = new PropertyDictionary<string, IApiServerInteraction, IApiServerResponse>(Interactions, (i) => i.Response);
        }

        public IApiServerInteraction this[string id] { get => Interactions[id]; }

        private ApiServerInteraction FindOrCreate(string id)
        {
            if(!Interactions.TryGetValue(id, out IApiServerInteraction apiServerInteraction))
            {
                apiServerInteraction = new ApiServerInteraction();

                Interactions.Add(id, apiServerInteraction);
            }

            return (ApiServerInteraction)apiServerInteraction;
        }
        public void Add(string id, IApiServerResponse response)
        {
            ApiServerInteraction interaction = FindOrCreate(id);

            if(interaction.Response != null)
            {
                throw new ArgumentException($"Api Interaction with id {id} has existing response");
            }

            interaction.Response = response;
        }

        public void Add(string id, IApiPayload request)
        {
            ApiServerInteraction interaction = FindOrCreate(id);

            if (interaction.Request != null)
            {
                throw new ArgumentException($"Api Interaction with id {id} has existing request");
            }

            interaction.Request = request;
        }

        public void Add(string id, IApiServerInteraction interaction)
        {
            if (Interactions.ContainsKey(id))
            {
                throw new ArgumentException($"Api Interaction with id {id} already exists");
            } else
            {
                Interactions.Add(id, interaction);
            }
        }
        public void Add(IApiServerInteraction interaction) => this.Add(interaction.Id, interaction);

        public IEnumerator<IApiServerInteraction> GetEnumerator()
        {
            foreach(KeyValuePair<string, IApiServerInteraction> kvp in Interactions)
            {
                yield return kvp.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IPropertyDictionary<string, IApiPayload> Requests { get; protected set; }
        public IPropertyDictionary<string, IApiServerResponse> Responses { get; protected set; }

        public (string Id, IApiPayload Request) Request(string id) => (id, Interactions.Single(k => k.Key == id).Value.Request);
        

        public (string Id, IApiServerResponse Response) Response(string id) => (id, Interactions.Single(k => k.Key == id).Value.Response);
    }
}
