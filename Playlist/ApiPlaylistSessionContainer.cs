﻿using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Abstractions.ObjectArrays;
using Penguin.Api.ObjectArrays;
using Penguin.Web;
using Penguin.Web.Abstractions;
using System;

namespace Penguin.Api.Playlist
{
    public class ApiPlaylistSessionContainer : IApiPlaylistSessionContainer, IDisposable
    {
        public IWebClient Client { get; set; } = new WebClientWrapper(new WebClientEx() { FollowRedirect = false });
        public bool DisposeAfterUse { get; set; }
        public IApiServerInteractionCollection Interactions { get; set; } = new ApiServerInteractionCollection();
        public IJavascriptEngine JavascriptEngine { get; set; } = new JavascriptEngineWrapper(new Jint.Engine());
        public IObjectArray SessionObjects { get; set; } = new ObjectArray();

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.Client.Dispose();
                    this.JavascriptEngine.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PlaylistSessionContainer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        #endregion IDisposable Support
    }
}