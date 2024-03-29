﻿using Penguin.Api.Abstractions.Interfaces;
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
        private readonly Dictionary<string, IApiServerInteraction> Interactions = new();

        public IPropertyDictionary<string, IApiPayload> Requests { get; protected set; }

        public IPropertyDictionary<string, IApiServerResponse> Responses { get; protected set; }

        public ApiServerInteractionCollection()
        {
            Requests = new PropertyDictionary<string, IApiServerInteraction, IApiPayload>(Interactions, (i) => i.Request);
            Responses = new PropertyDictionary<string, IApiServerInteraction, IApiServerResponse>(Interactions, (i) => i.Response);
        }

        public IApiServerInteraction this[string id] => Interactions[id];

        public void Add(string id, IApiServerResponse response)
        {
            ApiServerInteraction interaction = FindOrCreate(id);

            if (interaction.Response != null)
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
            }
            else
            {
                Interactions.Add(id, interaction);
            }
        }

        public void Add(IApiServerInteraction interaction)
        {
            if (interaction is null)
            {
                throw new ArgumentNullException(nameof(interaction));
            }

            Add(interaction.Id, interaction);
        }

        public IEnumerator<IApiServerInteraction> GetEnumerator()
        {
            foreach (KeyValuePair<string, IApiServerInteraction> kvp in Interactions)
            {
                yield return kvp.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public (string Id, IApiPayload Request) Request(string id)
        {
            return (id, Interactions.Single(k => k.Key == id).Value.Request);
        }

        public (string Id, IApiServerResponse Response) Response(string id)
        {
            return (id, Interactions.Single(k => k.Key == id).Value.Response);
        }

        private ApiServerInteraction FindOrCreate(string id)
        {
            if (!Interactions.TryGetValue(id, out IApiServerInteraction apiServerInteraction))
            {
                apiServerInteraction = new ApiServerInteraction();

                Interactions.Add(id, apiServerInteraction);
            }

            return (ApiServerInteraction)apiServerInteraction;
        }
    }
}