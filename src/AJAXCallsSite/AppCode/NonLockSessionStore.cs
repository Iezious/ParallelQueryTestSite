using System;
using System.Collections.Concurrent;

namespace AJAXCallsSite.AppCode
{
    public class NonLockSessionStore
    {
        private static NonLockSessionStore _instance;

        static NonLockSessionStore()
        {
            _instance = new NonLockSessionStore();
        }

        public static NonLockSessionStore Instance => _instance;

        public ConcurrentDictionary<string, NoLockingStoredData> Sessions { get; } =
            new ConcurrentDictionary<string, NoLockingStoredData>(Environment.ProcessorCount, 65003);
    }
}