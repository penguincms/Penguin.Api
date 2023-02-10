using Newtonsoft.Json.Linq;
using Penguin.Api.Shared;
using Penguin.Json;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.Json
{
    public class JsonPostPayload : ServerPostPayload<JsonString>
    {
        public JsonPostPayload()
        {
            Headers.Add("Accept", "application/json, text/plain, */*");
            Headers.Add("Content-Type", "application/json;charset=UTF-8");
        }

        public override void SetValue(string path, object Value, string newPropName) // Copied from response
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new System.ArgumentException("message", nameof(path));
            }

            if (path == "$")
            {
                JToken newToken = (Value as JToken) ?? JToken.Parse(Value.ToString());

                JsonString oldString = new(Body);

                if (oldString.IsValid &&
                    JToken.Parse(oldString) is JObject oldObject &&
                                   newToken is JObject newObject)
                {
                    foreach (JProperty prop in oldObject.Properties())
                    {
                        _ = newObject.Remove(prop.Name);

                        newObject.Add(prop.Name, prop.Value);
                    }
                }

                base.Body = newToken.ToString();

                return;
            }

            JObject destinationObject = JObject.Parse(base.Body);

            JToken destToken = destinationObject.SelectToken(path);

            if (destToken is null)
            {
                Queue<string> paths = new();

                foreach (string pathPart in path.Split('.'))
                {
                    if (pathPart == "$")
                    {
                        continue;
                    }

                    paths.Enqueue(pathPart);
                }

                string workingPath = string.Empty;
                JToken thisToken;
                JToken lastToken = destinationObject;

                while (paths.Any())
                {
                    string pathPart = paths.Dequeue();

                    workingPath += pathPart;

                    thisToken = destinationObject.SelectToken(workingPath);

                    if (thisToken is null)
                    {
                        thisToken = new JObject();

                        ((JObject)lastToken).Add(pathPart, thisToken);
                    }

                    lastToken = thisToken;

                    workingPath += ".";
                }

                destToken = lastToken;
            }

            JContainer target = destToken.Parent;

            object newValue;

            if (Value is JToken)
            {
                newValue = Value;
            }
            else
            {
                JsonString newString = Value?.ToString();

                newValue = newString.IsValid ? (JToken)newString : Value;
            }

            if (destToken.Parent.Type == JTokenType.Property)
            {
                JContainer pParent = destToken.Parent.Parent;
                destToken.Parent.Remove();
                pParent.Add(new JProperty(newPropName ?? (destToken.Parent as JProperty).Name, newValue));
            }
            else
            {
                destToken.Remove();
                target.Add(newValue);
            }

            base.Body = destinationObject.ToString();
        }

        public override bool TryGetValue(string path, out object value)
        {
            if (!base.TryGetValue(path, out value))
            {
                value = JToken.Parse(base.Body).SelectToken(path);
            }

            return true;
        }
    }
}