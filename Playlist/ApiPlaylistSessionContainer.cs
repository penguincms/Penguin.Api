﻿using Penguin.Api;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Web;
using Penguin.Web.Abstractions;
using System;
using System.Collections.Generic;

namespace MassageEnvy.Meevo.Playback
{
    public class ApiPlaylistSessionContainer : IApiPlaylistSessionContainer, IDisposable
    {
        public IWebClient Client { get; set; } = new WebClientWrapper(new WebClientEx() { FollowRedirect = false });
        public bool DisposeAfterUse { get; set; }
        public IJavascriptEngine JavascriptEngine { get; set; } = new JavascriptEngineWrapper(new Jint.Engine());
        public Dictionary<string, IApiServerResponse> PreviousResponses { get; set; } = new Dictionary<string, IApiServerResponse>();

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Client.Dispose();
                    JavascriptEngine.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
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