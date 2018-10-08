using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using System.Web.SessionState;

namespace AJAXCallsSite.AppCode
{
    public sealed class NoLockingSessionStoreProvider : SessionStateStoreProviderBase
    {
        private SessionStateItemExpireCallback _expireCallback;
        private int _timeout = 20;
        private Timer _cleaner;
        private bool _initialized = false;
        private bool _disposed = false;

        private NonLockSessionStore Store => NonLockSessionStore.Instance;
        
        
        public override void Initialize(string name, NameValueCollection config)
        {
            lock (string.Intern("NoLockingSessionStoreProvider"))
            {
                if(_initialized) return;
                _cleaner = new Timer(CleanCache, null, 60000, 60000);
                _initialized = true;
            }

            base.Initialize(name, config);
        }


        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            _expireCallback = expireCallback;
            return true;
        }


        public override void InitializeRequest(HttpContext context)
        {
            
        }

        public override void EndRequest(HttpContext context)
        {
            
        }

        public override void Dispose()
        {
            
        }


        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            if (Store.Sessions.TryGetValue(id, out var session))
            {
                locked = false;
                lockId = session.LockId;
                lockAge = DateTime.Now - session.AccessTime;
                actions = session.Initialized ? SessionStateActions.None : SessionStateActions.InitializeItem;

                return WrapData(session, context, _timeout);
            }
            else
            {
                locked = false;
                lockId = null;
                lockAge = TimeSpan.Zero;
                actions = SessionStateActions.InitializeItem;

                return WrapData(null, context, _timeout);
            }
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            if (Store.Sessions.TryGetValue(id, out var session))
            {
                locked = false;
                lockId = session.GetLock();
                lockAge = DateTime.Now - session.AccessTime;
                actions = session.Initialized ? SessionStateActions.None : SessionStateActions.InitializeItem;

                return WrapData(session, context, _timeout);
            }
            else
            {
                locked = false;
                lockId = null;
                lockAge = TimeSpan.Zero;
                actions = SessionStateActions.None;

                return null;
            }
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            if (Store.Sessions.TryGetValue(id, out var session))
            {
                session.Merge(item.Items);
            }
            else
            {
                session = new NoLockingStoredData();
                session.Merge(item.Items);
                Store.Sessions[id] = session;
            }
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            Store.Sessions.TryRemove(id, out var session);
            _expireCallback?.Invoke(id, WrapData(session, null, 0));
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            if(Store.Sessions.TryGetValue(id, out var session)) session.AccessTime = DateTime.Now;
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return WrapData(null, context, _timeout);
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            Store.Sessions[id] = new NoLockingStoredData() {Id = id};
        }
        
        private void CleanCache(object state)
        {
            var now = DateTime.Now;
            
            foreach (var session in Store.Sessions)
            {
                if((now - session.Value.AccessTime).TotalMinutes < _timeout) continue;

                Store.Sessions.TryRemove(session.Key, out var sessionValue);
                _expireCallback?.Invoke(session.Key, WrapData(sessionValue, null, 0));
            }
        }

        private SessionStateStoreData WrapData(NoLockingStoredData sessionValue, HttpContext context, int timeout)
        {
            return new SessionStateStoreData(
                sessionValue?.GetSessionData() ?? new NoLockingSessionData(),
                context != null ? SessionStateUtility.GetSessionStaticObjects(context) : null,
                timeout);
        }
    }
}