using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KTrie
{
	public class Trie<TKey, TValue> : IDictionary<IEnumerable<TKey>, TValue>, ICollection<KeyValuePair<IEnumerable<TKey>, TValue>>, IEnumerable<KeyValuePair<IEnumerable<TKey>, TValue>>, IEnumerable
	{
		private sealed class TrieEntryPrivate : IEnumerable<TKey>, IEnumerable
		{
			private IEnumerable<TKey> Key { get; }

			public TValue Value { get; set; }

			public TrieEntryPrivate(IEnumerable<TKey> key)
			{
				Key = key;
			}

			public IEnumerator<TKey> GetEnumerator()
			{
				return Key.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private readonly TrieSet<TKey> _trie;

		public int Count => _trie.Count;

		bool ICollection<KeyValuePair<IEnumerable<TKey>, TValue>>.IsReadOnly => false;

		public ICollection<IEnumerable<TKey>> Keys => _trie.ToList();

		public ICollection<TValue> Values => (from TrieEntryPrivate te in _trie
			select te.Value).ToArray();

		public TValue this[IEnumerable<TKey> key]
		{
			get
			{
				if (TryGetValue(key, out var value))
				{
					return value;
				}
				throw new KeyNotFoundException("The given key was not present in the trie.");
			}
			set
			{
				if (_trie.TryGetItem(key, out var item))
				{
					((TrieEntryPrivate)item).Value = value;
				}
				else
				{
					Add(key, value);
				}
			}
		}

		public Trie()
			: this((IEqualityComparer<TKey>)EqualityComparer<TKey>.Default)
		{
		}

		public Trie(IEqualityComparer<TKey> comparer)
		{
			_trie = new TrieSet<TKey>(comparer);
		}

		public IEnumerable<TrieEntry<TKey, TValue>> GetByPrefix(IEnumerable<TKey> prefix)
		{
			return from TrieEntryPrivate i in _trie.GetByPrefix(prefix)
				select new TrieEntry<TKey, TValue>(i, i.Value);
		}

		public IEnumerator<KeyValuePair<IEnumerable<TKey>, TValue>> GetEnumerator()
		{
			return (from TrieEntryPrivate i in _trie
				select new KeyValuePair<IEnumerable<TKey>, TValue>(i, i.Value)).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void ICollection<KeyValuePair<IEnumerable<TKey>, TValue>>.Add(KeyValuePair<IEnumerable<TKey>, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			_trie.Clear();
		}

		bool ICollection<KeyValuePair<IEnumerable<TKey>, TValue>>.Contains(KeyValuePair<IEnumerable<TKey>, TValue> item)
		{
			if (_trie.TryGetItem(item.Key, out var item2))
			{
				TValue value = ((TrieEntryPrivate)item2).Value;
				if (EqualityComparer<TValue>.Default.Equals(item.Value, value))
				{
					return true;
				}
			}
			return false;
		}

		public void CopyTo(KeyValuePair<IEnumerable<TKey>, TValue>[] array, int arrayIndex)
		{
			Array.Copy((from TrieEntryPrivate i in _trie
				select new KeyValuePair<IEnumerable<TKey>, TValue>(i, i.Value)).ToArray(), 0, array, arrayIndex, Count);
		}

		bool ICollection<KeyValuePair<IEnumerable<TKey>, TValue>>.Remove(KeyValuePair<IEnumerable<TKey>, TValue> item)
		{
			if (_trie.TryGetItem(item.Key, out var item2))
			{
				TValue value = ((TrieEntryPrivate)item2).Value;
				if (EqualityComparer<TValue>.Default.Equals(item.Value, value))
				{
					return Remove(item.Key);
				}
			}
			return false;
		}

		public bool ContainsKey(IEnumerable<TKey> key)
		{
			return _trie.Contains(key);
		}

		public void Add(IEnumerable<TKey> key, TValue value)
		{
			_trie.Add(new TrieEntryPrivate(key)
			{
				Value = value
			});
		}

		public bool Remove(IEnumerable<TKey> key)
		{
			return _trie.Remove(key);
		}

		public bool TryGetValue(IEnumerable<TKey> key, out TValue value)
		{
			IEnumerable<TKey> item;
			bool flag = _trie.TryGetItem(key, out item);
			value = (flag ? ((TrieEntryPrivate)item).Value : default(TValue));
			return flag;
		}
	}
}
