using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Shared;
using Penguin.Json;
using Penguin.Web.Abstractions.Interfaces;
using System;

namespace Penguin.Api.Json
{
    //Why is this even a thing?
    public class JsonGetItem : BaseGetItem<JsonGetPayload, JsonResponsePayload>
    {
        public override IApiServerInteraction<JsonGetPayload, JsonResponsePayload> Execute(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            _ = Container.JavascriptEngine.Execute("globalThis.Playlist = globalThis.Playlist || {};");
            _ = Container.JavascriptEngine.Execute($"globalThis.Playlist['{Id}'] = {{}};");

            IApiServerInteraction<JsonGetPayload, JsonResponsePayload> Interaction = base.Execute(Container);

            if (new JsonString(Interaction.Response.Body).IsValid)
            {
                _ = Container.JavascriptEngine.Execute($"globalThis.Playlist['{Id}'].Response = {Interaction.Response.Body};");
            }

            return Interaction;
        }

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<JsonGetPayload, JsonResponsePayload> item)
        {
            return TryCreate(request, response, "application/json", out item);
        }
    }
}