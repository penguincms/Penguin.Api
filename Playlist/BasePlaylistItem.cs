using Penguin.Api.Abstractions.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Penguin.Api.Playlist
{
    public abstract class BasePlaylistItem
    {
        [Display(Order = -1400)]
        public List<IExecutionCondition> Conditions { get; set; } = new List<IExecutionCondition>();

        [Display(Order = -1500)]
        public bool Enabled { get; set; } = true;

        public bool Executed { get; protected set; }

        [Display(Order = -2000)]
        public string Id { get; set; }

        [Display(Order = 500)]
        public List<ITransformation> Transformations { get; set; } = new List<ITransformation>();

        public abstract void Reset();
    }
}