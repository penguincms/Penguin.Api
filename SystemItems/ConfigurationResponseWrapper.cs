using Penguin.Api.Shared;
using System.Collections.Generic;

namespace Penguin.Api.SystemItems
{
    public class ConfigurationResponseWrapper : ApiServerResponse
    {
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();

        public void Add(string Key, object Value)
        {
            Values.Add(Key, Value);
        }

        public override void SetValue(string path, object Value, string newPropName)
        {
            if (newPropName != null)
            {
                _ = Values.Remove(path);
                Values.Add(newPropName, Value?.ToString());
            }
            else
            {
                Values[path] = Value?.ToString();
            }
        }

        public override bool TryGetValue(string path, out object value)
        {
            return base.TryGetValue(path, out value) || Values.TryGetValue(path, out value);
        }
    }
}