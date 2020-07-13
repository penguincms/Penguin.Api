using Penguin.Api.Shared;
using System.Collections.Generic;

namespace Penguin.Api.SystemItems
{
    public class ConfigurationResponseWrapper : ApiServerResponse
    {
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public void Add(string Key, string Value) => this.Values.Add(Key, Value);

        public override void SetValue(string path, object Value, string newPropName)
        {
            if (newPropName != null)
            {
                this.Values.Remove(path);
                this.Values.Add(newPropName, Value?.ToString());
            }
            else
            {
                this.Values[path] = Value?.ToString();
            }
        }

        public override bool TryGetValue(string path, out object value)
        {
            if (!base.TryGetValue(path, out value))
            {
                value = this.Values[path];
            }
            return true;
        }
    }
}