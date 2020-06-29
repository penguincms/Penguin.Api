using Penguin.Api.PostBody;
using Penguin.Api.Shared;
using System;

namespace Penguin.Api.Xml
{
    public class XmlPostPayload : ServerPostPayload<TextPostBody>
    {
        public XmlPostPayload()
        {
            this.Headers.Add("Accept", "text/xml, */*");
            this.Headers.Add("Content-Type", "text/xml");
        }

        public override void SetValue(string path, object Value, string newPropName) // Copied from response
        {
            throw new NotImplementedException();
        }

        public override bool TryGetValue(string path, out object value)
        {
            throw new NotImplementedException();
        }
    }
}