﻿using Penguin.Web.Abstractions.Interfaces;

namespace Penguin.Api
{
    public class JavascriptEngineWrapper : IJavascriptEngine
    {
        private Jint.Engine Engine { get; set; }

        public JavascriptEngineWrapper(Jint.Engine engine)
        {
            Engine = engine;
        }

        public string Execute(string toExecute)
        {
            return string.IsNullOrWhiteSpace(toExecute) ? null : Engine.Execute(toExecute).GetCompletionValue()?.ToString();
        }

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~JavascriptEngineWrapper()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        #endregion IDisposable Support
    }
}