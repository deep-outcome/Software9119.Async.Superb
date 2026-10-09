## Software9119.Async.Superb.Synchronization namespace

Namespace with types for synchronization.

### Types Provided

- [`AsyncLock`](./Locking/AsyncLock.cs) – access synchronization with memory synchronization
```csharp
private bool canSync = true;
private readonly AsyncLocker locker = new ();

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

ConcurrentBag<bool> bag = new();

Task[] test = Enumerable.Range ( 0, 100 )
.Select(async _ =>
{
    bool result = await Sync ();
    bag.Add ( result );
} )
.ToArray();

await Task.WhenAll ( test );

Assert.Equal ( 50, bag.Count ( x => x ) );
Assert.Equal ( 50, bag.Count ( x => !x ) );
````
- [`KeyGamma`](./Locking/KeyGamma.cs) – stack allocated unlocker for lock acquired by `AsyncLock`
- [`KeyAlpha`](./Locking/KeyAlpha.cs) – heap allocated unlocker for lock acquired by `AsyncLock`
