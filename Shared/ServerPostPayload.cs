using Penguin.SystemExtensions.Abstractions.Interfaces;

namespace Penguin.Api.Shared
{
    public abstract class ServerPostPayload<TBody> : ApiPayload where TBody : IConvertible<string>
    {
        public virtual TBody Body { get; set; }

        public override string ToString() => Body.ToString();
    }
}