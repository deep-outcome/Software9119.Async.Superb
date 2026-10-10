using System;
using System.Diagnostics.CodeAnalysis;

namespace Software9119.Async.Superb.Synchronization;

/// <summary>
/// Unlocks referential async lock.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Not thread safe.</item>
/// <item>Dot not duplicate, unless carefully handled, otherwise state clone allows for multiple unlocks.</item>
/// </list>
/// </remarks>
[SuppressMessage ( "Performance", "CA1815:Override equals and operator equals on value types",
  Justification = "Equality comparison unlikely." )]
public struct KeyGamma : IDisposable
{
  /// <summary>
  /// Async locker reference.
  /// </summary>
  [SuppressMessage ( "Design", "CA1051:Do not declare visible instance fields", Justification = "Type open to inheritance extension." )]
  internal AsyncLock? alRef;

  internal KeyGamma ( AsyncLock? alRef ) => this.alRef = alRef;

  /// <summary>
  /// Determines referential async lock existence.
  /// </summary>
  /// <returns>
  /// <see langword="false"/> when waiting for lock entrance timed out
  /// or when <see cref="AsyncLock"/> is disposed already.
  /// </returns>
  readonly public bool Locked => alRef != null;

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
  public void Dispose () => _ = Unlock ();

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
}
