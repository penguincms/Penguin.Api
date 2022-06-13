using Penguin.Web.Abstractions;
using Penguin.Web.Abstractions.Interfaces;
using System.Net;

namespace Penguin.Api
{
    public class WebClientWrapper : IWebClient
    {
        public WebHeaderCollection Headers => this.Client.Headers;
        public WebHeaderCollection ResponseHeaders => this.Client.ResponseHeaders;
        private WebClient Client { get; set; }

        public WebClientWrapper(WebClient client) => this.Client = client;

        public string DownloadString(string url) => this.Client.DownloadString(url);

        public string UploadString(string url, string data) => this.Client.UploadString(url, data);

        public string UploadString(string url, string method, string data) => this.Client.UploadString(url, method, data);

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
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WebClientWrapper()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        #endregion IDisposable Support
    }
}