using System;
using System.Diagnostics.CodeAnalysis;

namespace Software9119.Async.Superb.Synchronization;

/// <summary>
/// Unlocks referential async lock.
/// </summary>
/// <remarks>
/// Not thread safe, references should stay at stack only.
/// </remarks>
public class KeyAlpha : IDisposable
{
  /// <summary>
  /// Async locker reference.
  /// </summary>
  [SuppressMessage ( "Design", "CA1051:Do not declare visible instance fields", Justification = "Type open to inheritance extension." )]
  protected internal AsyncLock? alRef;

  internal KeyAlpha ( AsyncLock? alRef ) => this.alRef = alRef;

  /// <summary>
  /// Determines referential async lock existence.
  /// </summary>
  /// <returns>
  /// <see langword="false"/> when waiting for lock entrance timed out
  /// or when <see cref="AsyncLock"/> is disposed already.
  /// </returns>
  public bool Locked => alRef != null;

  /// <summary>
  /// Disposal.
  /// </summary>
  /// <remarks>Releases async lock, if unlocker is currently referring to ongoing one.</remarks>
  [SuppressMessage (
  "Design",
  "CA1063:Implement IDisposable Correctly",
  Justification = @"More reliable as it does not rely on base.Dispose(true) call from derived type,
    reliable unless derived class explicitly declares 'new public void Dispose()' which is unlikely."
)]
  public void Dispose ()
  {
    _ = Unlock ();

    GC.SuppressFinalize ( this );
    Dispose ( true );
  }

  /// <summary>
  /// Releases async lock, if unlocker is currently referring to ongoing one.
  /// </summary>
  /// <returns><see langword="false"/> if, there is no lock to release.</returns>
  public bool Unlock ()
  {
    if (alRef is AsyncLock locker)
    {
      alRef = null;
      return locker.Unlock ();
    }

    return false;
  }

  /// <summary>
  /// Disposal with option to specify managed+unmanaged (true) or just unmanaged resources (false) disposal.
  /// </summary>
  virtual protected void Dispose ( bool disposing ) { }
}
