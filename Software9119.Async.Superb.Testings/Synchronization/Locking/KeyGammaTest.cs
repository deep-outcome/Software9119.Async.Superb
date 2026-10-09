using Software9119.Async.Superb.Synchronization;

namespace Software9119.Async.Superb.Testings.Synchronization.Locking;

sealed public class KeyGammaTest
{

  [Fact]
  public void SafeDefault ()
  {
    {
      KeyGamma test = default;

      Assert.False ( test.Locked );
      Assert.False ( test.Unlock () );
      test.Dispose ();
    }

    {
      KeyGamma test = default;

      test.Dispose ();
      Assert.False ( test.Locked );
      Assert.False ( test.Unlock () );
    }
  }

  [Fact]
  public void SafeNullReference ()
  {
    {
      KeyGamma test = new ( null! );

      Assert.False ( test.Locked );
      Assert.False ( test.Unlock () );
      test.Dispose ();
    }

    {
      KeyGamma test = new ( null! );

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
      KeyGamma test = new ( locker );

      Assert.True ( test.Locked );
      // event wait handle set usually suceeds, and succeeds even when set already
      Assert.True ( test.Unlock () );

      Assert.False ( test.Locked );
      Assert.False ( test.Unlock () );
      test.Dispose ();
    }

    {
      KeyGamma test = await locker.GammaAsync ();

      Assert.True ( test.Locked );
      test.Dispose ();

      Assert.False ( test.Locked );
      Assert.False ( test.Unlock () ); ;

      test.Dispose ();
    }
  }
}
