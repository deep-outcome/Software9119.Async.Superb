using Software9119.Async.Superb.Extension;

using System.Diagnostics.CodeAnalysis;

namespace Software9119.Async.Superb.Testings;

sealed public class WaitHandleExtensionsTests_WaitOneAsync
{
  [Fact]
  public void NullEventWaitHandle_ArgumentNullException ()
  {
    Action test = () => EventWaitHandleExtension.WaitOneAsync ((EventWaitHandle)null!, default(TimeSpan), default);
    ArgumentNullException e = Assert.Throws<ArgumentNullException> ( test );

    Assert.Equal ( "Wait handle cannot be null. (Parameter 'ewh')", e.Message );
  }

  [Fact]
  [SuppressMessage ( "Reliability", "CA2025:Do not pass 'IDisposable' instances into unawaited tasks", Justification = "It's waited." )]
  public void CancellationRequested_WhenWaitingForSignal_TaskIsCancelled ()
  {
    using EventWaitHandle ewh = new (false, EventResetMode.AutoReset);
    using CancellationTokenSource cts = new ();

    Task<bool> test = ewh.WaitOneAsync (Timeout.InfiniteTimeSpan, cts.Token);
    // ensure maximum progress
    _ = Task.Run ( test.Wait );

    cts.Cancel ();

    AggregateException aggregateException = Assert.Throws<AggregateException> (test.Wait);

    Exception inner = Assert.Single ( aggregateException.InnerExceptions );
    Assert.Equal ( typeof ( TaskCanceledException ), inner.GetType () );
  }

  [Fact]
  [SuppressMessage ( "Reliability", "CA2025:Do not pass 'IDisposable' instances into unawaited tasks", Justification = "It's waited." )]
  public void CancellationRequested_BeforeEveryLogic_TaskIsCancelled ()
  {
    using EventWaitHandle ewh = new (false, EventResetMode.AutoReset);
    using CancellationTokenSource cts = new ();
    cts.Cancel ();

    Task<bool> test = ewh.WaitOneAsync (Timeout.InfiniteTimeSpan, cts.Token);

    AggregateException aggregateException = Assert.Throws<AggregateException> (test.Wait);

    Exception inner = Assert.Single ( aggregateException.InnerExceptions );
    Assert.Equal ( typeof ( TaskCanceledException ), inner.GetType () );
  }

  [Fact]
  async public Task EventWaitHandleNotSignaled_ImmediateTimeout_TimedOut ()
  {
    using EventWaitHandle ewh = new (false, EventResetMode.AutoReset);
    Assert.False ( await ewh.WaitOneAsync ( TimeSpan.Zero, default ( CT ) ) );
  }

  [Fact]
  async public Task EventWaitHandleNotSignaled_SomeTimeout_TimedOut ()
  {
    using EventWaitHandle ewh = new (false, EventResetMode.AutoReset);
    TimeSpan timeout = TimeSpan.FromMilliseconds(250);
    Assert.False ( await ewh.WaitOneAsync ( timeout, default ( CT ) ) );
  }

  [Fact]
  async public Task EventWaitHandleSignaled_WaitCompleted ()
  {
    using EventWaitHandle ewh = new (true, EventResetMode.AutoReset);
    Assert.True ( await ewh.WaitOneAsync ( TimeSpan.Zero, CancellationToken.None ) );

    Assert.False ( ewh.WaitOne ( 0 ) );
  }

  [Fact]
  [SuppressMessage ( "Reliability", "CA2025:Do not pass 'IDisposable' instances into unawaited tasks", Justification = "It's waited." )]
  async public Task EventWaitHandleSignaledWhenWaiting_WaitCompleted ()
  {
    using EventWaitHandle ewh = new (false, EventResetMode.AutoReset);

    Task<bool> waiting = ewh.WaitOneAsync ( (TimeSpan?)null, CancellationToken.None );
    Assert.False ( waiting.IsCompleted );

    await Task.Delay ( 750 );
    Assert.False ( waiting.IsCompleted );

    Assert.True ( ewh.Set () );
    Assert.True ( await waiting );
  }

  [Fact]
  [SuppressMessage ( "Reliability", "CA2025:Do not pass 'IDisposable' instances into unawaited tasks", Justification = "It's waited." )]
  async public Task UsageSimulation_Works ()
  {
    using EventWaitHandle ewh = new (true, EventResetMode.AutoReset);
    Assert.True ( await ewh.WaitOneAsync ( Timeout.InfiniteTimeSpan, CancellationToken.None ) );

    Assert.False ( await ewh.WaitOneAsync ( 125, CancellationToken.None ) );
    Assert.True ( ewh.Set () );

    Assert.True ( await ewh.WaitOneAsync ( 0, CancellationToken.None ) );

    Task<bool> wait = ewh.WaitOneAsync ( -1, CancellationToken.None );
    Assert.False ( wait.IsCompleted );

    Assert.True ( ewh.Set () );
    Assert.True ( await wait );
  }
}