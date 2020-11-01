using System;

namespace Memory.API
{
    public class APIObject : IDisposable
    {
        private readonly int n;

        public APIObject(int n)
        {
            this.n = n;

            MagicAPI.Allocate(n);
        }

        #region IDisposable Support

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        ~APIObject()
        {
            Dispose(false);
        }

        public void Dispose(bool disposing)
        {
            if (disposed) return;

            MagicAPI.Free(n);
            disposed = true;
        }

        #endregion
    }
}