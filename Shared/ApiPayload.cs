﻿using Penguin.Api.Abstractions.Interfaces;
using Penguin.Extensions.Strings;
using Penguin.Web;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{
    public abstract class ApiPayload : IApiPayload
    {
        public HttpHeaderCollection Headers { get; set; } = new HttpHeaderCollection();
        IDictionary<string, string> IApiPayload.Headers => this.Headers;
        public string Url { get; set; }

        public virtual void SetValue(string path, object Value) => this.SetValue(path, Value, null);

        public abstract void SetValue(string path, object Value, string newPropName);

        public override string ToString() => string.Empty;

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

                value = this.Headers[HeaderName];

                return true;
            }

            return false;
        }
    }
}