using System.Collections.Generic;

namespace Unity.Collections;

internal class NativeHeapDebugView<T, U> where T : unmanaged where U : unmanaged, IComparer<T>
{
	private UnsafeHeap<T, U> _heap;

	public int Count => _heap.Count;

	public int Capacity => _heap.Capacity;

	public U Comparator => _heap.Comparator;

	public unsafe T[] Items
	{
		get
		{
			T[] array = new T[_heap.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _heap.Data->Heap[i].Item;
			}
			return array;
		}
	}

	public NativeHeapDebugView(UnsafeHeap<T, U> heap)
	{
		_heap = heap;
	}
}
