using Software9119.Async.Superb.Extension;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Software9119.Async.Superb.Synchronization;

/// <summary>
/// Thread safe non-blocking exlusive access with memory synchronization between threads.
/// </summary>
public class AsyncLock : IDisposable
{
  /// <summary>
  /// <see cref="EventWaitHandle"/> that is used for synced access.
  /// </summary>
  [SuppressMessage ( "Design", "CA1051:Do not declare visible instance fields", Justification = "Type open to inheritance extension." )]
  readonly protected internal AutoResetEvent are = new(true);

#if !NET8_0
  readonly Lock locker = new();
#else
  readonly object locker = new ();
#endif

  internal bool disposed;

  /// <returns><see langword="true"/> when current instance is disposed already.</returns>
  public bool IsDisposed
  {
    get
    {
      lock (locker)
      {
        return disposed;
      }
    }
  }

  /// <remarks>
  /// <list type="bullet">
  /// <item>Use <see cref="TimeSpan.Zero"/> for immediate timeout.</item>
  /// <item>Use <see cref="Timeout.InfiniteTimeSpan"/> for infinite timeout.</item>
  /// <item>Default timeout is infinite.</item>
  /// <item>
  /// <see cref="KeyGamma.Locked"/> property returns <see langword="false"/> when wait timed out or
  /// when this instance is disposed already.
  /// </item>
  /// </list>  
  /// </remarks>
  /// <returns>Asynchronously <see cref="KeyGamma"/> which releases lock upon disposal.</returns>
  async public ValueTask<KeyGamma> GammaAsync ( TimeSpan? maxWaitTime = null, CancellationToken ct = default )
  {
    try
    {
      if (await are.WaitOneAsync ( maxWaitTime, ct ).ConfigureAwait ( false ))
      {
        // data are already synced by system or CLR but w/o expressed guarantee
        // for consumer code, issue barrier explicitly
        Thread.MemoryBarrier ();
        return new KeyGamma ( this );
      }
    }
    catch (ObjectDisposedException)
    {
    }

    return new KeyGamma ( null );
  }

  /// <remarks>
  /// <list type="bullet">
  /// <item>Use <c>0</c> for immediate timeout.</item>
  /// <item>Use <c>-1</c> for infinite timeout.</item>
  /// <item>
  /// <see cref="KeyGamma.Locked"/> property returns <see langword="false"/> when wait timed out or
  /// when this instance is disposed already.
  /// </item>
  /// </list>  
  /// </remarks>
  /// <returns>Asynchronously <see cref="KeyGamma"/> which releases lock upon disposal.</returns>
  async public ValueTask<KeyGamma> GammaAsync ( int maxWaitTimeMilliSecs, CancellationToken ct = default )
  {
    try
    {
      if (await are.WaitOneAsync ( maxWaitTimeMilliSecs, ct ).ConfigureAwait ( false ))
      {
        // data are already synced by system or CLR but w/o expressed guarantee
        // for consumer code, issue barrier explicitly
        Thread.MemoryBarrier ();
        return new KeyGamma ( this );
      }
    }
    catch (ObjectDisposedException)
    {
    }

    return new KeyGamma ( null );
  }

  /// <remarks>
  /// <list type="bullet">
  /// <item>Use <see cref="TimeSpan.Zero"/> for immediate timeout.</item>
  /// <item>Use <see cref="Timeout.InfiniteTimeSpan"/> for infinite timeout.</item>
  /// <item>Default timeout is infinite.</item>
  /// <item>
  /// <see cref="KeyAlpha.Locked"/> property returns <see langword="false"/> when wait timed out or
  /// when this instance is disposed already.
  /// </item>
  /// </list>
  /// </remarks>
  /// <returns>Asynchronously <see cref="KeyAlpha"/> which releases lock upon disposal.</returns>
  async public Task<KeyAlpha> AlphaAsync ( TimeSpan? maxWaitTime = null, CancellationToken ct = default )
  {
    try
    {
      if (await are.WaitOneAsync ( maxWaitTime, ct ).ConfigureAwait ( false ))
      {
        // data are already synced by system or CLR but w/o expressed guarantee
        // for consumer code, issue barrier explicitly
        Thread.MemoryBarrier ();
        return new KeyAlpha ( this );
      }
    }
    catch (ObjectDisposedException)
    {
    }

    return new KeyAlpha ( null );
  }

  /// <remarks>
  /// <list type="bullet">
  /// <item>Use <c>0</c> for immediate timeout.</item>
  /// <item>Use <c>-1</c> for infinite timeout.</item>
  /// <item>
  /// <see cref="KeyAlpha.Locked"/> property returns <see langword="false"/> when wait timed out or
  /// when this instance is disposed already.
  /// </item>
  /// </list>
  /// </remarks>
  /// <returns>Asynchronously <see cref="KeyAlpha"/> which releases lock upon disposal.</returns>
  async public Task<KeyAlpha> AlphaAsync ( int maxWaitTimeMilliSecs, CancellationToken ct = default )
  {
    try
    {
      if (await are.WaitOneAsync ( maxWaitTimeMilliSecs, ct ).ConfigureAwait ( false ))
      {
        // data are already synced by system or CLR but w/o expressed guarantee
        // for consumer code, issue barrier explicitly
        Thread.MemoryBarrier ();
        return new KeyAlpha ( this );
      }
    }
    catch (ObjectDisposedException)
    {
    }

    return new KeyAlpha ( null );
  }

  /// <summary>
  /// Releases async lock.
  /// </summary>
  protected internal bool Unlock ()
  {
    try
    {
      return are.Set ();
    }
    catch (ObjectDisposedException)
    {
      return false;
    }
  }

  /// <summary>
  /// Disposes all managed resources, calls <see cref="GC.SuppressFinalize(object)"/> with 
  /// <see langword="this"/> and then calls <see cref="Dispose(bool)"/>
  /// with <see langword="true"/>.
  /// </summary>
  [SuppressMessage (
    "Design",
    "CA1063:Implement IDisposable Correctly",
    Justification = @"More reliable as it does not rely on base.Dispose(true) call from derived type, 
    reliable unless derived class explicitly declares 'new public void Dispose()' which is unlikely."
  )]
  public void Dispose ()
  {
    lock (locker)
    {
      if (disposed) return;

      are.Dispose ();
      GC.SuppressFinalize ( this );
      disposed = true;

      Dispose ( true );
    }
  }

  /// <summary>
  /// Disposal with option to specify managed+unmanaged (true) or just unmanaged resources (false) disposal.
  /// </summary>  
  virtual protected void Dispose ( bool disposing ) { }
}
