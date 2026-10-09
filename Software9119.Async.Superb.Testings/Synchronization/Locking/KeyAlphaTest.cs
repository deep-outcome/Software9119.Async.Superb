using Software9119.Async.Superb.Synchronization;

namespace Software9119.Async.Superb.Testings.Synchronization.Locking;

sealed public class KeyAlphaTest
{

  [Fact]
  public void SafeNullReference ()
  {
    {
      KeyAlpha test = new ( null! );
      Assert.False ( test.Locked );
      Assert.False ( test.Unlock () );
      test.Dispose ();
    }

    {
      KeyAlpha test = new ( null! );
      test.Dispose ();
      Assert.False ( test.Locked );
      Assert.False ( test.Unlock () );
    }
  }

  [Fact]
  async public Task Locked ()
  {
    using AsyncLock locker = new ();

    {
      KeyAlpha test = new ( locker );

      Assert.True ( test.Locked );
      // event wait handle set usually suceeds, and succeeds even when set already
      Assert.True ( test.Unlock () );

      Assert.False ( test.Locked );
      Assert.False ( test.Unlock () );
      test.Dispose ();
    }

    {
      KeyAlpha test = await locker.AlphaAsync ();
      Assert.True ( test.Locked );
      test.Dispose ();

      Assert.False ( test.Locked );
      Assert.False ( test.Unlock () ); ;

      test.Dispose ();
    }
  }
}
