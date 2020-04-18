using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Playlist;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{
    public class SleepPlaylistItem : IPlaylistItem
    {
        private static Random random = new Random();
        public bool Executed { get; }
        public string Id { get; set; }
        public int MaximumWaitMs { get; set; }
        public int MinimumWaitMs { get; set; }
        public bool Enabled { get; set; }
        IApiPayload IPlaylistItem.Request { get; } = new EmptyPayload();
        IApiServerResponse IPlaylistItem.Response { get; } = new EmptyResponsePayload();
        List<ITransformation> IPlaylistItem.Transformations { get; set; } = new List<ITransformation>();
        string IPlaylistItem.Url { get; set; } = string.Empty;

        public IApiServerInteraction Execute(IApiPlaylistSessionContainer Container)
        {
            System.Threading.Thread.Sleep(random.Next(MinimumWaitMs, MaximumWaitMs));

            return new ApiEmptyServerInteraction(); 
        }

        public void Reset()
        {

        }

        public override string ToString() => $"Sleep {MinimumWaitMs}-{MaximumWaitMs}";
    }
}