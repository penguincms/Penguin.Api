using Penguin.Api.Abstractions.Enumerations;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Forms;
using Penguin.Api.Shared;
using Penguin.Extensions.Strings;
using Penguin.Json.Extensions;
using Penguin.Web.Abstractions.Interfaces;
using Penguin.Web.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;

namespace Penguin.Api.Playlist
{
    public abstract class BasePlaylistItem
    {

    
        [Display(Order = -1500)]
        public bool Enabled { get; set; } = true;
        public bool Executed { get; protected set; }
        [Display(Order = - 2000)]
        public string Id { get; set; }

        [Display(Order = 500)]
        public List<ITransformation> Transformations { get; set; } = new List<ITransformation>();


        public abstract void Reset();
    }
}