namespace Unity.Collections;

internal struct HeapNode<T> where T : unmanaged
{
	public T Item;

	public int TableIndex;
}
