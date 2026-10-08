using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Software9119.Async.Superb.Extension;

/// <summary>
/// <see cref="EventWaitHandle "/> extension methods.
/// </summary>
static public class EventWaitHandleExtension
{
  /// <remarks>
  /// <list type="bullet">
  /// <item>Use <see cref="TimeSpan.Zero"/> for immediate timeout.</item>
  /// <item>Use <see cref="Timeout.InfiniteTimeSpan"/> for infinite timeout.</item>
  /// </list>
  /// Default timeout is infinite.
  /// </remarks>
  /// <exception cref="ArgumentNullException">When <paramref name="ewh"/> is <see langword="null"/>.</exception>
  /// <exception cref="TaskCanceledException">When cancelled before receiving signal.</exception>
  /// <exception cref="OverflowException">
  /// When <paramref name="maxWaitTime"/> cannot be converted to <see cref="int"/> milliseconds.
  /// </exception>
  /// <returns><see langword="true"/> when signaled, <see langword="false"/> when timed out.</returns>
  static public Task<bool> WaitOneAsync ( this EventWaitHandle ewh, TimeSpan? maxWaitTime = null, CT ct = default )
  {
    double totalMillis = (maxWaitTime ?? Timeout.InfiniteTimeSpan).TotalMilliseconds;

    return WaitOneAsync ( ewh, Convert.ToInt32 ( totalMillis, CultureInfo.InvariantCulture ), ct );
  }

  /// <remarks>
  /// <list type="bullet">
  /// <item>Use <c>0</c> for immediate timeout.</item>
  /// <item>Use <c>-1</c> for infinite timeout.</item>
  /// </list>
  /// </remarks>
  /// <exception cref="ArgumentNullException">When <paramref name="ewh"/> is <see langword="null"/>.</exception>
  /// <exception cref="TaskCanceledException">When cancelled before receiving signal.</exception>
  /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxWaitTime"/> is less than <c>-1</c>.</exception>
  /// <returns><see langword="true"/> when signaled, <see langword="false"/> when timed out.</returns>
  static public Task<bool> WaitOneAsync ( this EventWaitHandle ewh, int maxWaitTime = Timeout.Infinite, CT ct = default )
  {
    if (ewh == null)
      throw new ArgumentNullException ( paramName: nameof ( ewh ), "Wait handle cannot be null." );

    TaskCompletionSource<bool> completionSrc  = new (TaskCreationOptions.RunContinuationsAsynchronously);
    CancellationTokenRegistration callbackReg = default;

    bool nonCancelable = ct == CancellationToken.None;
    if (nonCancelable == false)
    {
      Action<object?, CancellationToken> callback = ( tcs, token ) => _ = ((TaskCompletionSource<bool>) tcs!).TrySetCanceled ( token );
      callbackReg = ct.UnsafeRegister ( callback, completionSrc );
    }

    RegisteredWaitHandle? rwh = null;
    try
    {
      if (nonCancelable || ct.IsCancellationRequested == false)
      {
        rwh = ThreadPool.RegisterWaitForSingleObject
        (
          ewh,
          ( _, timedOut ) => _ = completionSrc.TrySetResult ( !timedOut ),
          null,
          maxWaitTime,
          true
        );
      }
    }
    catch
    {
      // callback registration disposal doesn't have to wait for long running callback, await does not suit well
      // CA1849  When inside a Task-returning method, use the async version of methods, if they exist.
#pragma warning disable CA1849
      callbackReg.Dispose ();
#pragma warning restore CA1849
      throw;
    }

    Task<bool> completion = completionSrc.Task;
    _ = completion.ContinueWith
    (
      t =>
      {
        callbackReg.Dispose ();

        if (t.Status != TaskStatus.RanToCompletion)
          _ = rwh?.Unregister ( null );
      },
      CancellationToken.None,
      TaskContinuationOptions.None,
      TaskScheduler.Default
    );

    return completion;
  }
}

