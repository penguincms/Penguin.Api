using Penguin.Api.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Api.Shared
{
    public class ApiEmptyServerInteraction : IApiServerInteraction<EmptyPayload, EmptyResponsePayload>
    {
        public EmptyPayload Request { get; } = new EmptyPayload();
        public EmptyResponsePayload Response { get; } = new EmptyResponsePayload();
        public string Id { get; set; }

        IApiPayload IApiServerInteraction.Request => this.Request;
        IApiServerResponse IApiServerInteraction.Response => this.Response;
    }
}
