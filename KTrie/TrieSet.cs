using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KTrie
{
	public class TrieSet<T> : ICollection<IEnumerable<T>>, IEnumerable<IEnumerable<T>>, IEnumerable
	{
		internal sealed class TrieNode
		{
			public bool IsTerminal { get; set; }

			public T Key { get; }

			public IEnumerable<T> Item { get; set; }

			public IDictionary<T, TrieNode> Children { get; }

			public TrieNode Parent { get; set; }

			public TrieNode(T key, IEqualityComparer<T> comparer)
			{
				Key = key;
				Children = new Dictionary<T, TrieNode>(comparer);
			}
		}

		private readonly IEqualityComparer<T> _comparer;

		private readonly TrieNode _root;

		public int Count { get; private set; }

		bool ICollection<IEnumerable<T>>.IsReadOnly => false;

		public TrieSet()
			: this((IEqualityComparer<T>)EqualityComparer<T>.Default)
		{
		}

		public TrieSet(IEqualityComparer<T> comparer)
		{
			_comparer = comparer;
			_root = new TrieNode(default(T), comparer);
		}

		public IEnumerator<IEnumerable<T>> GetEnumerator()
		{
			return GetAllNodes(_root).Select(GetFullKey).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(IEnumerable<T> key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			TrieNode trieNode = _root;
			foreach (T item in key)
			{
				trieNode = AddItem(trieNode, item);
			}
			if (trieNode.IsTerminal)
			{
				throw new ArgumentException($"An element with the same key already exists: '{key}'", "key");
			}
			trieNode.IsTerminal = true;
			trieNode.Item = key;
			Count++;
		}

		public void AddRange(IEnumerable<IEnumerable<T>> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			foreach (IEnumerable<T> item in collection)
			{
				Add(item);
			}
		}

		public void Clear()
		{
			_root.Children.Clear();
			Count = 0;
		}

		public bool Contains(IEnumerable<T> item)
		{
			return GetNode(item)?.IsTerminal ?? false;
		}

		public void CopyTo(IEnumerable<T>[] array, int arrayIndex)
		{
			Array.Copy(GetAllNodes(_root).Select(GetFullKey).ToArray(), 0, array, arrayIndex, Count);
		}

		public bool Remove(IEnumerable<T> key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			TrieNode node = GetNode(key);
			if (node == null)
			{
				return false;
			}
			if (!node.IsTerminal)
			{
				return false;
			}
			RemoveNode(node);
			return true;
		}

		public bool TryGetItem(IEnumerable<T> key, out IEnumerable<T> item)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			TrieNode node = GetNode(key);
			item = null;
			if (node == null)
			{
				return false;
			}
			if (!node.IsTerminal)
			{
				return false;
			}
			item = node.Item;
			return true;
		}

		internal bool TryGetNode(IEnumerable<T> key, out TrieNode node)
		{
			node = GetNode(key);
			if (node == null)
			{
				return false;
			}
			if (!node.IsTerminal)
			{
				return false;
			}
			return true;
		}

		public IEnumerable<IEnumerable<T>> GetByPrefix(IEnumerable<T> prefix)
		{
			if (prefix == null)
			{
				throw new ArgumentNullException("prefix");
			}
			TrieNode value = _root;
			foreach (T item in prefix)
			{
				if (!value.Children.TryGetValue(item, out value))
				{
					return Enumerable.Empty<IEnumerable<T>>();
				}
			}
			return GetByPrefix(value);
		}

		private static IEnumerable<TrieNode> GetAllNodes(TrieNode node)
		{
			foreach (KeyValuePair<T, TrieNode> child in node.Children)
			{
				if (child.Value.IsTerminal)
				{
					yield return child.Value;
				}
				foreach (TrieNode allNode in GetAllNodes(child.Value))
				{
					if (allNode.IsTerminal)
					{
						yield return allNode;
					}
				}
			}
		}

		private static IEnumerable<IEnumerable<T>> GetByPrefix(TrieNode node)
		{
			Stack<TrieNode> stack = new Stack<TrieNode>();
			TrieNode current = node;
			while (stack.Count > 0 || current != null)
			{
				if (current != null)
				{
					if (current.IsTerminal)
					{
						yield return GetFullKey(current);
					}
					using IEnumerator<KeyValuePair<T, TrieNode>> enumerator = current.Children.GetEnumerator();
					current = (enumerator.MoveNext() ? enumerator.Current.Value : null);
					while (enumerator.MoveNext())
					{
						stack.Push(enumerator.Current.Value);
					}
				}
				else
				{
					current = stack.Pop();
				}
			}
		}

		private static IEnumerable<T> GetFullKey(TrieNode node)
		{
			return node.Item;
		}

		private TrieNode GetNode(IEnumerable<T> key)
		{
			TrieNode value = _root;
			foreach (T item in key)
			{
				if (!value.Children.TryGetValue(item, out value))
				{
					return null;
				}
			}
			return value;
		}

		private void RemoveNode(TrieNode node)
		{
			Remove(node);
			Count--;
		}

		private TrieNode AddItem(TrieNode node, T key)
		{
			if (!node.Children.TryGetValue(key, out var value))
			{
				value = new TrieNode(key, _comparer)
				{
					Parent = node
				};
				node.Children.Add(key, value);
			}
			return value;
		}

		private void Remove(TrieNode node, T key)
		{
			foreach (KeyValuePair<T, TrieNode> child in node.Children)
			{
				if (_comparer.Equals(key, child.Key))
				{
					node.Children.Remove(child);
					break;
				}
			}
		}

		private void Remove(TrieNode node)
		{
			while (true)
			{
				node.IsTerminal = false;
				if (node.Children.Count == 0 && node.Parent != null)
				{
					Remove(node.Parent, node.Key);
					if (!node.Parent.IsTerminal)
					{
						node = node.Parent;
						continue;
					}
					break;
				}
				break;
			}
		}
	}
}
