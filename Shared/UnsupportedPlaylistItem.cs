using Penguin.Api.Abstractions.Interfaces;
using Penguin.Web;
using Penguin.Web.Headers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Api.Shared
{
    public class UnsupportedPlaylistItem : IPlaylistItem
    {
        public UnsupportedPlaylistItem(HttpServerInteraction interaction) : this(interaction?.Request, interaction?.Response)
        {

        }

        public UnsupportedPlaylistItem(HttpServerRequest hrequest, HttpServerResponse hresponse)
        {
            if (hrequest is null)
            {
                throw new ArgumentNullException(nameof(hrequest));
            }

            if (hresponse is null)
            {
                throw new ArgumentNullException(nameof(hresponse));
            }

            EmptyPayload request = new EmptyPayload();

            Request = request;

            foreach (HttpHeader h in hrequest.Headers)
            {
                request.Headers.Add(h);
            }

            GenericResponsePayload response = new GenericResponsePayload();

            Response = response;

            foreach (HttpHeader h in hresponse.Headers)
            {
                response.Headers.Add(h);
            }
        }

        bool IPlaylistItem.Enabled => false;
        bool IPlaylistItem.Executed => false;
        public string Id { get; set; }
        public IApiPayload Request { get; }
        public IApiServerResponse Response { get; }
        List<ITransformation> IPlaylistItem.Transformations { get; set; }
        public string Url { get; set; }

        public IApiServerInteraction Execute(IApiPlaylistSessionContainer Container)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {

        }
    }
}
