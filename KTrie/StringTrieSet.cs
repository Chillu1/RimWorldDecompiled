using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KTrie
{
	public class StringTrieSet : ICollection<string>, IEnumerable<string>, IEnumerable
	{
		private readonly TrieSet<char> _trie;

		public int Count => _trie.Count;

		bool ICollection<string>.IsReadOnly => false;

		public StringTrieSet()
			: this(EqualityComparer<char>.Default)
		{
		}

		public StringTrieSet(IEqualityComparer<char> comparer)
		{
			_trie = new TrieSet<char>(comparer);
		}

		public IEnumerable<string> GetByPrefix(string prefix)
		{
			return from c in _trie.GetByPrefix(prefix)
				select new string(c.ToArray());
		}

		public IEnumerator<string> GetEnumerator()
		{
			return _trie.Select((IEnumerable<char> c) => new string(c.ToArray())).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(string item)
		{
			_trie.Add(item);
		}

		public void AddRange(IEnumerable<string> item)
		{
			_trie.AddRange(item);
		}

		public void Clear()
		{
			_trie.Clear();
		}

		public bool Contains(string item)
		{
			return _trie.Contains(item);
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			Array.Copy(_trie.Select((IEnumerable<char> c) => new string(c.ToArray())).ToArray(), 0, array, arrayIndex, Count);
		}

		public bool Remove(string item)
		{
			return _trie.Remove(item);
		}
	}
}
