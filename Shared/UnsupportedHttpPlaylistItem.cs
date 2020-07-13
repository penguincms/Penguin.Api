using Penguin.Api.Abstractions.Interfaces;
using Penguin.Web;
using Penguin.Web.Abstractions.Interfaces;
using System;

namespace Penguin.Api.Shared
{
    public class UnsupportedHttpPlaylistItem : HttpPlaylistItem<EmptyPayload, GenericResponsePayload>
    {
        private string Method { get; set; }
        private string RequestContentType { get; set; }
        private string ResponseContentType { get; set; }

        public UnsupportedHttpPlaylistItem()
        {
        }

        public UnsupportedHttpPlaylistItem(HttpServerInteraction interaction) : this(interaction?.Request, interaction?.Response)
        {
        }

        public UnsupportedHttpPlaylistItem(HttpServerRequest hrequest, HttpServerResponse hresponse)
        {
            if (hrequest is null)
            {
                throw new ArgumentNullException(nameof(hrequest));
            }

            if (hresponse is null)
            {
                throw new ArgumentNullException(nameof(hresponse));
            }

            this.Method = hrequest.Method;
            this.RequestContentType = hrequest.ContentType;
            this.ResponseContentType = hresponse?.ContentType;

            this.SetupHttpPlaylistItem(this, hrequest, hresponse);

            this.Enabled = false;
        }

        public override IApiServerInteraction<EmptyPayload, GenericResponsePayload> Execute(IApiPlaylistSessionContainer Container) => throw new NotImplementedException();

        public override string GetBody(IApiPlaylistSessionContainer Container, EmptyPayload request) => throw new NotImplementedException();

        public override string ToString() => $"UNSUPPORTED {this.Method} ({this.RequestContentType} => {this.ResponseContentType})";

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<EmptyPayload, GenericResponsePayload> item)
        {
            item = null;
            return false;
        }
    }
}