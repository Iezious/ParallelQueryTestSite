using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.EnterpriseServices.Internal;
using System.Threading;
using System.Web.SessionState;

namespace AJAXCallsSite.AppCode
{
    public class NoLockingStoredData
    {
        public string Id { get; set; }
        public long ObjectStamp { get; } = DateTime.Now.Ticks;  
        public bool Initialized { get; set; } = false;
        public DateTime AccessTime { get; set; } = DateTime.Now;
        private int _lockId = 1;

        private ConcurrentDictionary<string, object> _storage = new ConcurrentDictionary<string, object>();

        public ISessionStateItemCollection GetSessionData()
        {
            AccessTime = DateTime.Now;
            return new NoLockingSessionData(_storage);
        }

        public void Merge(ISessionStateItemCollection session)
        {
            AccessTime = DateTime.Now;
            
            switch (session)
            {
                case NoLockingSessionData nlSession:
                    MergeLog(nlSession);
                    break;

                default:
                    MergeKeys(session);
                    break;
            }
        }

        private void MergeLog(NoLockingSessionData session)
        {
            foreach (var dataAction in session.ActionLog)
            {
                switch (dataAction.Action)
                {
                    case NoLockingSessionData.DataAction.Clear:
                        _storage.Clear();
                        break;
                    
                    case NoLockingSessionData.DataAction.Delete:
                        _storage.TryRemove(dataAction.Key, out _);
                        break;
                    
                    case NoLockingSessionData.DataAction.Set:
                        _storage[dataAction.Key] = dataAction.Data;
                        break;
                }
            }
        }

        private void MergeKeys(ISessionStateItemCollection session)
        {
            var hkes = new HashSet<string>();

            foreach (string sessionKey in session.Keys)
            {
                hkes.Add(sessionKey);
                _storage[sessionKey] = session[sessionKey];
            }

            foreach (var storageKey in _storage.Keys)
            {
                if(hkes.Contains(storageKey)) continue;

                _storage.TryRemove(storageKey, out _);
            }
        }

        public int LockId => _lockId;

        public int GetLock()
        {
            return Interlocked.Increment(ref _lockId);
        }
        
        
    }
}