using Penguin.Api.Abstractions.Interfaces;
using System;
using System.Collections.Generic;

namespace Penguin.Api.Shared
{
    public class SleepPlaylistItem : IPlaylistItem
    {
        private static readonly Random random = new Random();
        List<IExecutionCondition> IPlaylistItem.Conditions { get; set; } = new List<IExecutionCondition>();
        public bool Enabled { get; set; }
        public bool Executed { get; }
        public string Id { get; set; }
        public int MaximumWaitMs { get; set; }
        public int MinimumWaitMs { get; set; }
        List<ITransformation> IPlaylistItem.Transformations { get; set; } = new List<ITransformation>();

        public void Execute(IApiPlaylistSessionContainer Container) => System.Threading.Thread.Sleep(random.Next(this.MinimumWaitMs, this.MaximumWaitMs));

        public void Reset()
        {
        }

        public override string ToString() => $"Sleep {this.MinimumWaitMs}-{this.MaximumWaitMs}";
    }
}