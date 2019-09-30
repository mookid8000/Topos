using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
#pragma warning disable 1998

namespace Topos.Kafka.Tests
{
    class InMemExclusiveLockBandit
    {
        readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public async Task<IDisposable> GrabLock(string key)
        {
            while (true)
            {
                if (_locks.TryAdd(key, null))
                {
                    Console.WriteLine($"GRAB LOCK {key}");
                    return new LockReleaser(key, this);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }

        class LockReleaser : IDisposable
        {
            readonly string _key;
            readonly InMemExclusiveLockBandit _bandit;

            public LockReleaser(string key, InMemExclusiveLockBandit bandit)
            {
                _key = key;
                _bandit = bandit;
            }

            public void Dispose() => _bandit.ReleaseLock(_key);
        }

        void ReleaseLock(string key)
        {
            _locks.TryRemove(key, out _);

            Console.WriteLine($"RELEASE LOCK {key}");
        }
    }
}