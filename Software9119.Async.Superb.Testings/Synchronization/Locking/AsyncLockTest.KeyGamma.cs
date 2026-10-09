using Software9119.Async.Superb.Synchronization;

using System.Diagnostics.CodeAnalysis;

namespace Software9119.Async.Superb.Testings.Synchronization.Locking;

[SuppressMessage (
  "Performance",
  "CA1849",
  Justification = "No slow CT callback is registered during tests."
)]
[SuppressMessage ( "Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Okay." )]
sealed public class AsyncLockTestKeyGamma
{
  const int int_immediate = 0, int_infinite = -1, int_some = 250;
  static readonly TimeSpan
    ts_immediate = TimeSpan.FromMilliseconds(0),
    ts_infinite = TimeSpan.FromMilliseconds(-1),
    ts_some = TimeSpan.FromMilliseconds(250);


  static async Task AssertTaskCanceledException ( ValueTask<KeyGamma> test )
  {
    _ = await Assert.ThrowsAsync<TaskCanceledException> ( async () => await test );
  }

  [Fact]
  async public Task CancellationRequested_WhenWaitingForSignal_TaskIsCancelled__TimeSpan ()
  {

    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();

    _ = await locker.GammaAsync ();

    ValueTask<KeyGamma> test = locker.GammaAsync( ts_infinite, cts.Token);
    await Task.Delay ( 300 );

    cts.Cancel ();

    await AssertTaskCanceledException ( test );
  }

  [Fact]
  async public Task CancellationRequested_WhenWaitingForSignal_TaskIsCancelled__Int32 ()
  {

    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();

    _ = await locker.GammaAsync ( int_infinite );

    ValueTask<KeyGamma> test = locker.GammaAsync( int_infinite, cts.Token);
    await Task.Delay ( 300 );
    
    cts.Cancel ();

    await AssertTaskCanceledException ( test );
  }

  [Fact]
  async public Task CancellationRequested_BeforeEveryLogic_TaskIsCancelled__TimeSpan ()
  {
    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();
    cts.Cancel ();

    ValueTask<KeyGamma> test = locker.GammaAsync( ts_infinite, cts.Token);

    await AssertTaskCanceledException ( test );
  }

  [Fact]
  async public Task CancellationRequested_BeforeEveryLogic_TaskIsCancelled__Int32 ()
  {
    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();
    cts.Cancel ();

    ValueTask<KeyGamma> test = locker.GammaAsync( int_infinite, cts.Token);

    await AssertTaskCanceledException ( test );
  }

  [Fact]
  async public Task __ImmediateTimeout_TimedOut__TimeSpan ()
  {
    using AsyncLock locker = new ();

    _ = await locker.GammaAsync ( ts_infinite, CancellationToken.None );
    KeyGamma test = await locker.GammaAsync( ts_immediate, CT.None);

    Assert.False ( test.Locked );
  }

  [Fact]
  async public Task __ImmediateTimeout_TimedOut__Int32 ()
  {
    using AsyncLock locker = new ();

    _ = await locker.GammaAsync ( int_infinite, CancellationToken.None );
    KeyGamma test = await locker.GammaAsync( int_immediate, CT.None);

    Assert.False ( test.Locked );
  }

  [Fact]
  async public Task __SomeTimeout_TimedOut__TimeSpan ()
  {
    using AsyncLock locker = new ();

    _ = await locker.GammaAsync ( ts_infinite, CancellationToken.None );

    KeyGamma test = await locker.GammaAsync( ts_some, CT.None);

    Assert.False ( test.Locked );
  }

  [Fact]
  async public Task __SomeTimeout_TimedOut__Int32 ()
  {
    using AsyncLock locker = new ();

    _ = await locker.GammaAsync ( int_infinite, CancellationToken.None );

    KeyGamma test = await locker.GammaAsync( int_some, CT.None);

    Assert.False ( test.Locked );
  }

  [Fact]
  async public Task UsageSimulation_Works__TimeSpan ()
  {
    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();

    KeyGamma A = await locker.GammaAsync ( ts_infinite, CT.None );
    Assert.True ( A.Locked );

    KeyGamma B = await locker.GammaAsync( ts_some, CT.None );
    Assert.False ( B.Locked );

    A.Dispose ();
    KeyGamma C = await locker.GammaAsync( ts_immediate, CT.None );
    Assert.True ( C.Locked );

    ValueTask<KeyGamma> D = locker.GammaAsync( ts_infinite, CT.None );

    await Task.Delay ( 250 );
    Assert.False ( D.IsCompleted );

    Assert.True ( C.Unlock () );
    Assert.True ( (await D).Locked );

    ValueTask<KeyGamma> E = locker.GammaAsync( ts_infinite, cts.Token);

    cts.Cancel ();
    await AssertTaskCanceledException ( E );
  }

  [Fact]
  async public Task UsageSimulation_Works__Int32 ()
  {
    using AsyncLock locker = new ();
    using CancellationTokenSource cts = new ();

    KeyGamma A = await locker.GammaAsync ( int_infinite, CancellationToken.None );
    Assert.True ( A.Locked );

    KeyGamma B = await locker.GammaAsync( int_some, CT.None );
    Assert.False ( B.Locked );

    A.Dispose ();
    KeyGamma C = await locker.GammaAsync( int_immediate, CT.None );
    Assert.True ( C.Locked );

    ValueTask<KeyGamma> D = locker.GammaAsync( int_infinite, CT.None );

    await Task.Delay ( 250 );
    Assert.False ( D.IsCompleted );

    Assert.True ( C.Unlock () );
    Assert.True ( (await D).Locked );

    ValueTask<KeyGamma> E = locker.GammaAsync( int_infinite, cts.Token);

    cts.Cancel ();
    await AssertTaskCanceledException ( E );
  }

  [Fact]
  async public Task Unlock_Unlocks ()
  {
    using AsyncLock locker = new ();
    Assert.True ( locker.Unlock () );// set succeeds independently on being set or reset

    KeyGamma A = await locker.GammaAsync ( int_infinite);
    Assert.True ( A.Locked );

    ValueTask<KeyGamma> B = locker.GammaAsync( int_infinite);
    await Task.Delay ( 250 );
    Assert.False ( B.IsCompleted );

    Assert.True ( locker.Unlock () );
    Assert.True ( (await B).Locked );
  }

  [Fact]
  async public Task SafeDisposedInstance ()
  {
    using AsyncLock locker = new ();
    Assert.False ( locker.IsDisposed );

    KeyGamma A = await locker.GammaAsync(ts_infinite);
    
    locker.Dispose ();
    SpinWait.SpinUntil ( () => locker.disposed );
    Assert.False ( locker.Unlock () );

    KeyGamma B = await locker.GammaAsync(ts_infinite);
    Assert.False ( B.Locked );

    KeyGamma C = await locker.GammaAsync(int_infinite);
    Assert.False ( C.Locked );
  }
}
