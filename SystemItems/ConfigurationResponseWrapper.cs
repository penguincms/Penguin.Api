using Penguin.Api.Shared;
using System.Collections.Generic;

namespace Penguin.Api.SystemItems
{
    public class ConfigurationResponseWrapper : ApiServerResponse
    {
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public void Add(string Key, string Value)
        {
            Values.Add(Key, Value);
        }

        public override void SetValue(string path, string Value, string newPropName)
        {
            if (newPropName != null)
            {
                Values.Remove(path);
                Values.Add(newPropName, Value);
            }
            else
            {
                Values[path] = Value;
            }
        }

        public override bool TryGetValue(string path, out string value)
        {
            if (!base.TryGetValue(path, out value))
            {
                value = Values[path];
            }
            return true;
        }
    }
}