using System;
using System.Xml.Linq;

namespace Penguin.Api.Shared
{
    public class GenericResponsePayload : ApiServerResponse
    {
        private bool parseXml = true;

        public override string Body
        {
            get
            {
                if (!(this.Headers["Content-Type"]?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) ?? true))
                {
                    this.parseXml = false;
                }

                if (!this.parseXml || string.IsNullOrWhiteSpace(base.Body))
                {
                    return base.Body;
                }

                try
                {
                    return XDocument.Parse(base.Body)?.ToString();
                }
                catch (Exception)
                {
                    this.parseXml = false;
                    return base.Body;
                }
            }
            set => base.Body = value;
        }

        public override void SetValue(string path, object Value, string newPropName) => throw new NotImplementedException();
    }
}