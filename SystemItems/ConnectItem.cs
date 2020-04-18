using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Shared;
using System;
using System.Collections.Generic;

namespace Penguin.Api.SystemItems
{
    public class ConnectItem : IPlaylistItem
    {
        public bool Executed { get; }
        public string Id { get; set; }
        public EmptyPayload Request { get; } = new EmptyPayload();
        IApiPayload IPlaylistItem.Request => Request;
        public IApiServerResponse Response { get; } = new EmptyResponsePayload();
        public List<ITransformation> Transformations { get; set; } = new List<ITransformation>();
        public string Url { get; set; }
        public bool Enabled => false;

        public IApiServerInteraction Execute(IApiPlaylistSessionContainer Container)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {

        }
    }
}