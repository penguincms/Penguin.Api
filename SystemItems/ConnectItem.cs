using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Shared;
using Penguin.Web.Abstractions.Interfaces;
using System;
using System.Collections.Generic;

namespace Penguin.Api.SystemItems
{
    public class ConnectItem : HttpPlaylistItem<EmptyPayload, GenericResponsePayload>
    {
        public override IApiServerInteraction<EmptyPayload, GenericResponsePayload> Execute(IApiPlaylistSessionContainer Container)
        {
            throw new NotImplementedException();
        }

        public override string GetBody(IApiPlaylistSessionContainer Container, EmptyPayload request)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "CONNECT";
        }

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<EmptyPayload, GenericResponsePayload> item)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Method == "CONNECT")
            {
                item = new ConnectItem();
                item.Enabled = false;
                return true;
            } else
            {
                item = null;
                return false;
            }
        }
    }
}