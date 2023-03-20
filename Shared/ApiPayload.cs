using Loxifi;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Extensions.String;
using Penguin.Web;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{
    public abstract class ApiPayload : IApiPayload
    {
        public HttpHeaderCollection Headers { get; set; } = new HttpHeaderCollection();

        IDictionary<string, string> IApiPayload.Headers => Headers;

        public string Url { get; set; }

        public virtual void SetValue(string path, object Value)
        {
            SetValue(path, Value, null);
        }

        public abstract void SetValue(string path, object Value, string newPropName);

        public override string ToString()
        {
            return string.Empty;
        }

        public virtual bool TryGetValue(string path, out object value)
        {
            if (path is null)
            {
                throw new System.ArgumentNullException(nameof(path));
            }

            value = null;

            if (path.StartsWith("$Headers", StringComparison.OrdinalIgnoreCase))
            {
                string HeaderName = path.From(".");

                value = Headers[HeaderName];

                return true;
            }

            return false;
        }
    }
}