using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.SessionState;

namespace AJAXCallsSite.AppCode
{
    public class NoLockingSessionData : NameObjectCollectionBase, ISessionStateItemCollection
    {
        public enum DataAction { Set, Delete, Clear};
        
        public struct SessionDataAction
        {
            public DataAction Action;
            public string Key;
            public object Data;
        }

        private readonly List<SessionDataAction> _actionLog = new List<SessionDataAction>();

        public NoLockingSessionData(IEnumerable<KeyValuePair<string, object>> data)
        {
            foreach (var pair in data)
            {
                BaseAdd(pair.Key, pair.Value);
            }
        }

        public NoLockingSessionData()
        {
            
        }

        public IEnumerable<SessionDataAction> ActionLog => _actionLog;
        
        public void Remove(string name)
        {
            BaseRemove(name);
            _actionLog.Add(new SessionDataAction() {Action = DataAction.Delete, Key = name, Data = null});
            Dirty = true;
        }

        public void RemoveAt(int index)
        {
            Remove(BaseGetKey(index));
        }

        public void Clear()
        {
            BaseClear();
            _actionLog.Add(new SessionDataAction {Action = DataAction.Clear});
            Dirty = true;
        }

        public object this[string name]
        {
            get => BaseGet(name);
            set
            {
                BaseSet(name, value);
                _actionLog.Add(new SessionDataAction {Action = DataAction.Set, Data = value, Key = name});
                Dirty = true;
            }
        }

        public object this[int index]
        {
            get => BaseGet(index);
            set => this[BaseGetKey(index)] = value;
        }

        public bool Dirty { get; set; } = false;
    }
}