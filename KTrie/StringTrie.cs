using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KTrie
{
	public class StringTrie<TValue> : IDictionary<string, TValue>, ICollection<KeyValuePair<string, TValue>>, IEnumerable<KeyValuePair<string, TValue>>, IEnumerable
	{
		private readonly Trie<char, TValue> _trie;

		public int Count => _trie.Count;

		public ICollection<string> Keys => _trie.Keys.Select((IEnumerable<char> i) => new string(i.ToArray())).ToArray();

		public ICollection<TValue> Values => _trie.Values.ToArray();

		bool ICollection<KeyValuePair<string, TValue>>.IsReadOnly => false;

		public TValue this[string key]
		{
			get
			{
				return _trie[key];
			}
			set
			{
				_trie[key] = value;
			}
		}

		public StringTrie(IEqualityComparer<char> comparer)
		{
			_trie = new Trie<char, TValue>(comparer);
		}

		public StringTrie()
			: this((IEqualityComparer<char>)EqualityComparer<char>.Default)
		{
		}

		public void Add(string key, TValue value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			_trie.Add(key, value);
		}

		public void AddRange(IEnumerable<StringEntry<TValue>> collection)
		{
			foreach (StringEntry<TValue> item in collection)
			{
				_trie.Add(item.Key, item.Value);
			}
		}

		public void Clear()
		{
			_trie.Clear();
		}

		public bool ContainsKey(string key)
		{
			return _trie.ContainsKey(key);
		}

		public IEnumerable<StringEntry<TValue>> GetByPrefix(string prefix)
		{
			return from i in _trie.GetByPrefix(prefix)
				select new StringEntry<TValue>(new string(i.Key.ToArray()), i.Value);
		}

		public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
		{
			return _trie.Select((KeyValuePair<IEnumerable<char>, TValue> i) => new KeyValuePair<string, TValue>(new string(i.Key.ToArray()), i.Value)).GetEnumerator();
		}

		public bool Remove(string key)
		{
			return _trie.Remove(key);
		}

		public bool TryGetValue(string key, out TValue value)
		{
			return _trie.TryGetValue(key, out value);
		}

		void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> item)
		{
			return ((ICollection<KeyValuePair<IEnumerable<char>, TValue>>)_trie).Contains(new KeyValuePair<IEnumerable<char>, TValue>(item.Key, item.Value));
		}

		public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
		{
			Array.Copy(_trie.Select((KeyValuePair<IEnumerable<char>, TValue> i) => new KeyValuePair<string, TValue>(new string(i.Key.ToArray()), i.Value)).ToArray(), 0, array, arrayIndex, Count);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> item)
		{
			return ((ICollection<KeyValuePair<IEnumerable<char>, TValue>>)_trie).Remove(new KeyValuePair<IEnumerable<char>, TValue>(item.Key, item.Value));
		}
	}
}
