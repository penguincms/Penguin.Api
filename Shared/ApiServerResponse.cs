using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;
using System;

namespace Penguin.Api.Shared
{
    public abstract class ApiServerResponse : ApiPayload, IApiServerResponse
    {
        public virtual string Body { get; set; }

        public Exception Exception { get; set; }

        public ApiServerResponseStatus Status { get; set; }

        public override string ToString()
        {
            return Body;
        }
    }
}