using System.Diagnostics;
using System.Linq;

namespace System.Collections.Generic;

[DebuggerDisplay("Count = {Count}")]
public class PriorityQueue<TElement, TPriority>
{
	private (TElement Element, TPriority Priority)[] _nodes;

	private readonly IComparer<TPriority> _comparer;

	private int _size;

	private int _version;

	private const int Arity = 4;

	private const int Log2Arity = 2;

	public int Count => _size;

	public IComparer<TPriority> Comparer => _comparer ?? Comparer<TPriority>.Default;

	public PriorityQueue()
	{
		_nodes = Array.Empty<(TElement, TPriority)>();
		_comparer = InitializeComparer(null);
	}

	public PriorityQueue(int initialCapacity)
		: this(initialCapacity, (IComparer<TPriority>)null)
	{
	}

	public PriorityQueue(IComparer<TPriority> comparer)
	{
		_nodes = Array.Empty<(TElement, TPriority)>();
		_comparer = InitializeComparer(comparer);
	}

	public PriorityQueue(int initialCapacity, IComparer<TPriority> comparer)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentOutOfRangeException("initialCapacity");
		}
		_nodes = new(TElement, TPriority)[initialCapacity];
		_comparer = InitializeComparer(comparer);
	}

	public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items)
		: this(items, (IComparer<TPriority>)null)
	{
	}

	public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items, IComparer<TPriority> comparer)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		_nodes = items.ToArray();
		_size = _nodes.Length;
		_comparer = InitializeComparer(comparer);
		if (_size > 1)
		{
			Heapify();
		}
	}

	public void Enqueue(TElement element, TPriority priority)
	{
		int num = _size++;
		_version++;
		if (_nodes.Length == num)
		{
			Grow(num + 1);
		}
		if (_comparer == null)
		{
			MoveUpDefaultComparer((Element: element, Priority: priority), num);
		}
		else
		{
			MoveUpCustomComparer((Element: element, Priority: priority), num);
		}
	}

	public TElement Peek()
	{
		if (_size == 0)
		{
			throw new InvalidOperationException();
		}
		return _nodes[0].Element;
	}

	public TElement Dequeue()
	{
		if (_size == 0)
		{
			throw new InvalidOperationException();
		}
		TElement item = _nodes[0].Element;
		RemoveRootNode();
		return item;
	}

	public bool TryDequeue(out TElement element, out TPriority priority)
	{
		if (_size != 0)
		{
			(element, priority) = _nodes[0];
			RemoveRootNode();
			return true;
		}
		element = default(TElement);
		priority = default(TPriority);
		return false;
	}

	public bool TryPeek(out TElement element, out TPriority priority)
	{
		if (_size != 0)
		{
			(element, priority) = _nodes[0];
			return true;
		}
		element = default(TElement);
		priority = default(TPriority);
		return false;
	}

	public TElement EnqueueDequeue(TElement element, TPriority priority)
	{
		if (_size != 0)
		{
			(TElement, TPriority) tuple = _nodes[0];
			if (_comparer == null)
			{
				if (Comparer<TPriority>.Default.Compare(priority, tuple.Item2) > 0)
				{
					MoveDownDefaultComparer((Element: element, Priority: priority), 0);
					_version++;
					return tuple.Item1;
				}
			}
			else if (_comparer.Compare(priority, tuple.Item2) > 0)
			{
				MoveDownCustomComparer((Element: element, Priority: priority), 0);
				_version++;
				return tuple.Item1;
			}
		}
		return element;
	}

	public void EnqueueRange(IEnumerable<(TElement Element, TPriority Priority)> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		int num = 0;
		ICollection<(TElement, TPriority)> collection = items as ICollection<(TElement, TPriority)>;
		if (collection != null && (num = collection.Count) > _nodes.Length - _size)
		{
			Grow(_size + num);
		}
		if (_size == 0)
		{
			if (collection != null)
			{
				collection.CopyTo(_nodes, 0);
				_size = num;
			}
			else
			{
				int num2 = 0;
				(TElement, TPriority)[] nodes = _nodes;
				foreach (var (item, item2) in items)
				{
					if (nodes.Length == num2)
					{
						Grow(num2 + 1);
						nodes = _nodes;
					}
					nodes[num2++] = (item, item2);
				}
				_size = num2;
			}
			_version++;
			if (_size > 1)
			{
				Heapify();
			}
			return;
		}
		foreach (var (element, priority) in items)
		{
			Enqueue(element, priority);
		}
	}

	public void EnqueueRange(IEnumerable<TElement> elements, TPriority priority)
	{
		if (elements == null)
		{
			throw new ArgumentNullException("elements");
		}
		if (elements is ICollection<(TElement, TPriority)> { Count: var count })
		{
			int num = count;
			if (count > _nodes.Length - _size)
			{
				Grow(_size + num);
			}
		}
		if (_size == 0)
		{
			int num2 = 0;
			(TElement, TPriority)[] nodes = _nodes;
			foreach (TElement element in elements)
			{
				if (nodes.Length == num2)
				{
					Grow(num2 + 1);
					nodes = _nodes;
				}
				nodes[num2++] = (element, priority);
			}
			_size = num2;
			_version++;
			if (num2 > 1)
			{
				Heapify();
			}
			return;
		}
		foreach (TElement element2 in elements)
		{
			Enqueue(element2, priority);
		}
	}

	public void Clear()
	{
		Array.Clear(_nodes, 0, _size);
		_size = 0;
		_version++;
	}

	public int EnsureCapacity(int capacity)
	{
		if (_nodes.Length < capacity)
		{
			Grow(capacity);
			_version++;
		}
		return _nodes.Length;
	}

	public void TrimExcess()
	{
		int num = (int)((double)_nodes.Length * 0.9);
		if (_size < num)
		{
			Array.Resize(ref _nodes, _size);
			_version++;
		}
	}

	private void Grow(int minCapacity)
	{
		int num = 2 * _nodes.Length;
		if ((uint)num > 2147483647u)
		{
			num = int.MaxValue;
		}
		num = Math.Max(num, _nodes.Length + 4);
		if (num < minCapacity)
		{
			num = minCapacity;
		}
		Array.Resize(ref _nodes, num);
	}

	private void RemoveRootNode()
	{
		int num = --_size;
		_version++;
		if (num > 0)
		{
			(TElement, TPriority) node = _nodes[num];
			if (_comparer == null)
			{
				MoveDownDefaultComparer(node, 0);
			}
			else
			{
				MoveDownCustomComparer(node, 0);
			}
		}
		_nodes[num] = default((TElement, TPriority));
	}

	private static int GetParentIndex(int index)
	{
		return index - 1 >> 2;
	}

	private static int GetFirstChildIndex(int index)
	{
		return (index << 2) + 1;
	}

	private void Heapify()
	{
		(TElement, TPriority)[] nodes = _nodes;
		int parentIndex = GetParentIndex(_size - 1);
		if (_comparer == null)
		{
			for (int num = parentIndex; num >= 0; num--)
			{
				MoveDownDefaultComparer(nodes[num], num);
			}
		}
		else
		{
			for (int num2 = parentIndex; num2 >= 0; num2--)
			{
				MoveDownCustomComparer(nodes[num2], num2);
			}
		}
	}

	private void MoveUpDefaultComparer((TElement Element, TPriority Priority) node, int nodeIndex)
	{
		(TElement, TPriority)[] nodes = _nodes;
		while (nodeIndex > 0)
		{
			int parentIndex = GetParentIndex(nodeIndex);
			(TElement, TPriority) tuple = nodes[parentIndex];
			if (Comparer<TPriority>.Default.Compare(node.Priority, tuple.Item2) >= 0)
			{
				break;
			}
			nodes[nodeIndex] = tuple;
			nodeIndex = parentIndex;
		}
		nodes[nodeIndex] = node;
	}

	private void MoveUpCustomComparer((TElement Element, TPriority Priority) node, int nodeIndex)
	{
		IComparer<TPriority> comparer = _comparer;
		(TElement, TPriority)[] nodes = _nodes;
		while (nodeIndex > 0)
		{
			int parentIndex = GetParentIndex(nodeIndex);
			(TElement, TPriority) tuple = nodes[parentIndex];
			if (comparer.Compare(node.Priority, tuple.Item2) >= 0)
			{
				break;
			}
			nodes[nodeIndex] = tuple;
			nodeIndex = parentIndex;
		}
		nodes[nodeIndex] = node;
	}

	private void MoveDownDefaultComparer((TElement Element, TPriority Priority) node, int nodeIndex)
	{
		(TElement, TPriority)[] nodes = _nodes;
		int size = _size;
		int num;
		while ((num = GetFirstChildIndex(nodeIndex)) < size)
		{
			(TElement, TPriority) tuple = nodes[num];
			int num2 = num;
			int num3 = Math.Min(num + 4, size);
			while (++num < num3)
			{
				(TElement, TPriority) tuple2 = nodes[num];
				if (Comparer<TPriority>.Default.Compare(tuple2.Item2, tuple.Item2) < 0)
				{
					tuple = tuple2;
					num2 = num;
				}
			}
			if (Comparer<TPriority>.Default.Compare(node.Priority, tuple.Item2) <= 0)
			{
				break;
			}
			nodes[nodeIndex] = tuple;
			nodeIndex = num2;
		}
		nodes[nodeIndex] = node;
	}

	private void MoveDownCustomComparer((TElement Element, TPriority Priority) node, int nodeIndex)
	{
		IComparer<TPriority> comparer = _comparer;
		(TElement, TPriority)[] nodes = _nodes;
		int size = _size;
		int num;
		while ((num = GetFirstChildIndex(nodeIndex)) < size)
		{
			(TElement, TPriority) tuple = nodes[num];
			int num2 = num;
			int num3 = Math.Min(num + 4, size);
			while (++num < num3)
			{
				(TElement, TPriority) tuple2 = nodes[num];
				if (comparer.Compare(tuple2.Item2, tuple.Item2) < 0)
				{
					tuple = tuple2;
					num2 = num;
				}
			}
			if (comparer.Compare(node.Priority, tuple.Item2) <= 0)
			{
				break;
			}
			nodes[nodeIndex] = tuple;
			nodeIndex = num2;
		}
		nodes[nodeIndex] = node;
	}

	private static IComparer<TPriority> InitializeComparer(IComparer<TPriority> comparer)
	{
		if (typeof(TPriority).IsValueType)
		{
			if (comparer == Comparer<TPriority>.Default)
			{
				return null;
			}
			return comparer;
		}
		return comparer ?? Comparer<TPriority>.Default;
	}
}
