using Penguin.Web.Abstractions.Interfaces;
using System;

namespace Penguin.Api.Shared
{
    public class HttpGetItem : BaseGetItem<EmptyPayload, GenericResponsePayload>
    {
        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<EmptyPayload, GenericResponsePayload> item)
        {
            return TryCreate(request, response, (ContentType) => ContentType is null || !ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase), out item);
        }
    }
}