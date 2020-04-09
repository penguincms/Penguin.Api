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
            this.Headers.Add("Accept", "application/json, text/plain, */*");
            this.Headers.Add("Content-Type", "application/json;charset=UTF-8");
        }

        public override void SetValue(string path, string Value, string newPropName) // Copied from response
        {
            JObject destinationObject = JObject.Parse(base.Body);

            JToken destToken = destinationObject.SelectToken(path);

            if (destToken is null)
            {
                Queue<string> paths = new Queue<string>();

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

        public override bool TryGetValue(string path, out string value)
        {
            if (!base.TryGetValue(path, out value))
            {
                value = JToken.Parse(base.Body).SelectToken(path).ToString();
            }

            return true;
        }
    }
}