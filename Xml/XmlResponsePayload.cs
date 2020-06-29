using Penguin.Api.Shared;
using System;

namespace Penguin.Api.Xml
{
    public class XmlResponsePayload : ApiServerResponse
    {
        public override void SetValue(string path, object Value, string newPropName)// Copied from post
        {
            throw new NotImplementedException();
        }

        public override bool TryGetValue(string path, out object value)
        {
            throw new NotImplementedException();
        }
    }
}