namespace Unity.Collections;

internal struct HeapData<T, U> where T : unmanaged
{
	public int Count;

	public int Capacity;

	public unsafe HeapNode<T>* Heap;

	public unsafe TableValue* Table;

	public U Comparator;
}
