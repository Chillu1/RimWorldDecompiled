using System.Collections.Generic;

namespace Verse
{
	public class LRUCache<K, V>
	{
		private readonly Dictionary<K, LinkedListNode<(K, V)>> cache = new Dictionary<K, LinkedListNode<(K, V)>>();

		private readonly LinkedList<(K, V)> leastRecentList = new LinkedList<(K, V)>();

		private readonly int capacity;

		public LRUCache(int capacity)
		{
			this.capacity = capacity;
		}

		public bool TryGetValue(K key, out V result)
		{
			if (cache.TryGetValue(key, out var value))
			{
				result = value.Value.Item2;
				WasUsed(value);
				return true;
			}
			result = default(V);
			return false;
		}

		public void Add(K key, V value)
		{
			if (cache.Count > capacity)
			{
				RemoveLeastUsed();
			}
			LinkedListNode<(K, V)> linkedListNode = new LinkedListNode<(K, V)>((key, value));
			cache.Add(key, linkedListNode);
			leastRecentList.AddLast(linkedListNode);
		}

		public void Clear()
		{
			cache.Clear();
			leastRecentList.Clear();
		}

		private void WasUsed(LinkedListNode<(K, V)> node)
		{
			leastRecentList.Remove(node);
			leastRecentList.AddLast(node);
		}

		private void RemoveLeastUsed()
		{
			LinkedListNode<(K, V)> first = leastRecentList.First;
			if (first != null)
			{
				leastRecentList.RemoveFirst();
				cache.Remove(first.Value.Item1);
			}
		}
	}
}
