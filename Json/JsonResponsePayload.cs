using Newtonsoft.Json.Linq;
using Penguin.Api.Shared;
using Penguin.Json;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.Json
{
    public class JsonResponsePayload : ApiServerResponse
    {
        public override string Body
        {
            get => new JsonString(base.Body).Value;
            set => base.Body = value;
        }

        public override void SetValue(string path, object Value, string newPropName)// Copied from post
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new System.ArgumentException("path can not be null or whitespace", nameof(path));
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

            if (destToken.Parent.Type == JTokenType.Property)
            {
                JContainer pParent = destToken.Parent.Parent;
                destToken.Parent.Remove();
                pParent.Add(new JProperty(newPropName ?? (destToken.Parent as JProperty).Name, Value));
            }
            else
            {
                destToken.Remove();
                target.Add(Value);
            }

            base.Body = destinationObject.ToString();
        }

        public override bool TryGetValue(string path, out object value)
        {
            if (!base.TryGetValue(path, out value))
            {
                JToken token = JToken.Parse(base.Body).SelectToken(path);

                if (token is null)
                {
                    value = null;
                    return false;
                }
                else
                {
                    value = token;
                }
            }

            return true;
        }
    }
}