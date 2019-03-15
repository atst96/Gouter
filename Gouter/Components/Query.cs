using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal class Query : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _internalDict;

        public Query()
        {
            this._internalDict = new Dictionary<string, object>();
        }

        public object this[string key]
        {
            get => this._internalDict.TryGetValue(key, out var value) ? value : default;
            set => this.Add(key, value);
        }

        public ICollection<string> Keys => this._internalDict.Keys;

        public ICollection<object> Values => this._internalDict.Values;

        public int Count => this._internalDict.Count;

        public bool IsReadOnly => this._internalDict.IsReadOnly;

        public void Add(string key, object value)
        {
            if (this._internalDict.ContainsKey(key))
            {
                this._internalDict[key] = value;
            }
            else
            {
                this._internalDict.Add(key, value);
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this._internalDict.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return this._internalDict.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return this._internalDict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            this._internalDict.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this._internalDict.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return this._internalDict.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return this._internalDict.Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return this._internalDict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._internalDict.GetEnumerator();
        }
    }
}
