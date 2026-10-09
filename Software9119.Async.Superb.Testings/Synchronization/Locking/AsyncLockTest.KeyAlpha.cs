using Software9119.Async.Superb.Synchronization;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Software9119.Async.Superb.Testings.Synchronization.Locking;

[SuppressMessage (
  "Performance",
  "CA1849",
  Justification = "No slow CT callback is registered during tests."
)]
[SuppressMessage ( "Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Okay." )]
sealed public class AsyncLockTestKeyAlpha
{
  const int int_immediate = 0, int_infinite = -1, int_some = 250;
  static readonly TimeSpan
    ts_immediate = TimeSpan.FromMilliseconds(0),
    ts_infinite = TimeSpan.FromMilliseconds(-1),
    ts_some = TimeSpan.FromMilliseconds(250);


  static void AssertTaskCanceledException ( Task<KeyAlpha> test )
  {
    AggregateException aggregateException = Assert.Throws<AggregateException> (test.Wait);

    Exception innerException = Assert.Single (aggregateException.InnerExceptions );
    Assert.True ( innerException is TaskCanceledException );
  }

  [Fact]
  async public Task CancellationRequested_WhenWaitingForSignal_TaskIsCancelled__TimeSpan ()
  {

    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();

    _ = await locker.AlphaAsync ();

    Task<KeyAlpha> test = locker.AlphaAsync( ts_infinite, cts.Token);
    _ = Task.Run ( test.Wait );

    cts.Cancel ();

    AssertTaskCanceledException ( test );
  }

  [Fact]
  async public Task CancellationRequested_WhenWaitingForSignal_TaskIsCancelled__Int32 ()
  {

    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();

    _ = await locker.AlphaAsync ( int_infinite );

    Task<KeyAlpha> test = locker.AlphaAsync( int_infinite, cts.Token);
    _ = Task.Run ( test.Wait );

    cts.Cancel ();

    AssertTaskCanceledException ( test );
  }

  [Fact]
  async public Task CancellationRequested_BeforeEveryLogic_TaskIsCancelled__TimeSpan ()
  {
    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();
    cts.Cancel ();

    Task<KeyAlpha> test = locker.AlphaAsync( ts_infinite, cts.Token);

    AssertTaskCanceledException ( test );
  }

  [Fact]
  async public Task CancellationRequested_BeforeEveryLogic_TaskIsCancelled__Int32 ()
  {
    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();
    cts.Cancel ();

    Task<KeyAlpha> test = locker.AlphaAsync( int_infinite, cts.Token);

    AssertTaskCanceledException ( test );
  }

  [Fact]
  async public Task __ImmediateTimeout_TimedOut__TimeSpan ()
  {
    using AsyncLock locker = new ();

    _ = await locker.AlphaAsync ( ts_infinite, CancellationToken.None );
    KeyAlpha test = await locker.AlphaAsync( ts_immediate, CT.None);

    Assert.False ( test.Locked );
  }

  [Fact]
  async public Task __ImmediateTimeout_TimedOut__Int32 ()
  {
    using AsyncLock locker = new ();

    _ = await locker.AlphaAsync ( int_infinite, CancellationToken.None );
    KeyAlpha test = await locker.AlphaAsync( int_immediate, CT.None);

    Assert.False ( test.Locked );
  }

  [Fact]
  async public Task __SomeTimeout_TimedOut__TimeSpan ()
  {
    using AsyncLock locker = new ();

    _ = await locker.AlphaAsync ( ts_infinite, CancellationToken.None );

    KeyAlpha test = await locker.AlphaAsync( ts_some, CT.None);

    Assert.False ( test.Locked );
  }

  [Fact]
  async public Task __SomeTimeout_TimedOut__Int32 ()
  {
    using AsyncLock locker = new ();

    _ = await locker.AlphaAsync ( int_infinite, CancellationToken.None );

    KeyAlpha test = await locker.AlphaAsync( int_some, CT.None);

    Assert.False ( test.Locked );
  }

  [Fact]
  async public Task UsageSimulation_Works__TimeSpan ()
  {
    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();

    KeyAlpha A = await locker.AlphaAsync ( ts_infinite, CT.None );
    Assert.True ( A.Locked );

    KeyAlpha B = await locker.AlphaAsync( ts_some, CT.None );
    Assert.False ( B.Locked );

    A.Dispose ();
    KeyAlpha C = await locker.AlphaAsync( ts_immediate, CT.None );
    Assert.True ( C.Locked );

    Task<KeyAlpha> D = locker.AlphaAsync( ts_infinite, CT.None );

    await Task.Delay ( 250 );
    Assert.False ( D.IsCompleted );

    Assert.True ( C.Unlock () );
    Assert.True ( (await D).Locked );

    Task<KeyAlpha> E = locker.AlphaAsync( ts_infinite, cts.Token);

    cts.Cancel ();
    AssertTaskCanceledException ( E );
  }

  [Fact]
  async public Task UsageSimulation_Works__Int32 ()
  {
    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();

    KeyAlpha A = await locker.AlphaAsync ( int_infinite, CancellationToken.None );
    Assert.True ( A.Locked );

    KeyAlpha B = await locker.AlphaAsync( int_some, CT.None );
    Assert.False ( B.Locked );

    A.Dispose ();
    KeyAlpha C = await locker.AlphaAsync( int_immediate, CT.None );
    Assert.True ( C.Locked );

    Task<KeyAlpha> D = locker.AlphaAsync( int_infinite, CT.None );

    await Task.Delay ( 250 );
    Assert.False ( D.IsCompleted );

    Assert.True ( C.Unlock () );
    Assert.True ( (await D).Locked );

    Task<KeyAlpha> E = locker.AlphaAsync( int_infinite, cts.Token);

    cts.Cancel ();
    AssertTaskCanceledException ( E );
  }

  [Fact]
  async public Task Unlock_Unlocks ()
  {
    using AsyncLock locker = new ();
    Assert.True ( locker.Unlock () );// set succeeds independently on being set or reset

    KeyAlpha A = await locker.AlphaAsync ( int_infinite);
    Assert.True ( A.Locked );

    Task<KeyAlpha> B = locker.AlphaAsync( int_infinite);
    await Task.Delay ( 250 );
    Assert.False ( B.IsCompleted );

    Assert.True ( locker.Unlock () );
    Assert.True ( (await B).Locked );
  }

  [Fact]
  async public Task Disposal ()
  {
    using AsyncLock locker = new ();
    Assert.False ( locker.IsDisposed );
    Assert.False ( locker.disposed );
    Assert.True ( locker.are.Set () );

    locker.Dispose ();

    Assert.True ( locker.IsDisposed );
    Assert.True ( locker.disposed );
    _ = Assert.Throws<ObjectDisposedException> ( () => locker.are.Set () );
  }

  [Fact]
  async public Task SafeDisposedInstance ()
  {
    using AsyncLock locker = new ();
    Assert.False ( locker.IsDisposed );

    KeyAlpha A = await locker.AlphaAsync(ts_infinite);

    locker.Dispose ();

    SpinWait.SpinUntil ( () => locker.disposed );
    Assert.False ( locker.Unlock () );

    KeyAlpha B = await locker.AlphaAsync(ts_infinite);
    Assert.False ( B.Locked );

    KeyAlpha C = await locker.AlphaAsync(int_infinite);
    Assert.False ( C.Locked );
  }

  // readme

  [SuppressMessage ( "Style", "IDE0040:Remove accessibility modifiers", Justification = "Readme stylization." )]
  private bool canSync = true;
  [SuppressMessage ( "Style", "IDE0036:Order modifiers", Justification = "Readme stylization." )]
  [SuppressMessage ( "Style", "IDE0040:Remove accessibility modifiers", Justification = "Readme stylization." )]
  private readonly AsyncLock locker = new ();

  [SuppressMessage ( "Style", "IDE0036:Order modifiers", Justification = "Readme stylization." )]
  public async ValueTask<bool> Sync ()
  {
    using KeyAlpha unlocker = await locker.AlphaAsync ();
    if (canSync)
    {
      // … do work

      canSync = false;
      return true;
    }

    canSync = true;
    return false;
  }

  [Fact]
  [SuppressMessage ( "Style", "IDE0036:Order modifiers", Justification = "Readme stylization." )]
  public async Task ParallelCallShow ()
  {
    ConcurrentBag<bool> bag = new();

    Task[] test = Enumerable.Range ( 0, 100 )
    .Select ( async _ =>
    {
      bool result = await Sync ();
      bag.Add ( result );
    } )
    .ToArray ();

    await Task.WhenAll ( test );

    Assert.Equal ( 50, bag.Count ( x => x ) );
    Assert.Equal ( 50, bag.Count ( x => !x ) );
  }
}
