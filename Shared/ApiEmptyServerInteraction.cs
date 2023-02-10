using Penguin.Api.Abstractions.Interfaces;

namespace Penguin.Api.Shared
{
    public class ApiEmptyServerInteraction : IApiServerInteraction<EmptyPayload, EmptyResponsePayload>
    {
        public string Id { get; set; }

        public EmptyPayload Request { get; } = new EmptyPayload();

        IApiPayload IApiServerInteraction.Request => Request;

        public EmptyResponsePayload Response { get; } = new EmptyResponsePayload();

        IApiServerResponse IApiServerInteraction.Response => Response;
    }
}