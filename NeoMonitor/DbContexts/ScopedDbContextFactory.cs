using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NeoMonitor.DbContexts
{
    public sealed class ScopedDbContextFactory
    {
        private readonly IServiceScopeFactory _scopedFactory;

        public ScopedDbContextFactory(IServiceScopeFactory scopeFactory)
        {
            _scopedFactory = scopeFactory;
        }

        public ScopedDbContextWrapper<T> CreateDbContext<T>() where T : DbContext
        {
            var scope = _scopedFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<T>();
            return new ScopedDbContextWrapper<T>(scope, context);
        }
    }

    public sealed class ScopedDbContextWrapper<T> : IDisposable where T : DbContext
    {
        private readonly IServiceScope _scope;

        private bool disposedValue = false;

        /// <summary>
        /// Use ScopedDbContextWrapper's Dispose() instead of DbContext.Dispose()
        /// </summary>
        public T Context { get; private set; }

        public ScopedDbContextWrapper(IServiceScope scope, T context)
        {
            _scope = scope;
            Context = context;
        }

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _scope?.Dispose();
                    Context?.Dispose();
                }
                Context = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}