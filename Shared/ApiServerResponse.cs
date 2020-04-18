using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;

namespace Penguin.Api.Shared
{
    public abstract class ApiServerResponse : ApiPayload, IApiServerResponse
    {
        public virtual string Body { get; set; }
        public ApiServerResponseStatus Status { get; set; }
        public override string ToString()
        {
            return Body;
        }
    }
}