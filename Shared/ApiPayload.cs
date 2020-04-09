using Penguin.Api.Abstractions.Interfaces;
using Penguin.Extensions.Strings;
using Penguin.Web;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{
    public abstract class ApiPayload : IApiPayload
    {
        public HttpHeaderCollection Headers { get; set; } = new HttpHeaderCollection();
        IDictionary<string, string> IApiPayload.Headers => Headers;

        public virtual void SetValue(string path, string Value) => SetValue(path, Value, null);

        public abstract void SetValue(string path, string Value, string newPropName);

        public virtual bool TryGetValue(string path, out string value)
        {
            value = null;

            if (path.StartsWith("$Headers"))
            {
                string HeaderName = path.From(".");

                value = Headers[HeaderName];

                return true;
            }

            return false;
        }
    }
}