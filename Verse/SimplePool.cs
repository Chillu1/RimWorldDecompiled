using System.Collections.Generic;

namespace Verse;

public static class SimplePool<T> where T : new()
{
	private static readonly Queue<T> freeItems = new Queue<T>();

	public static int FreeItemsCount => freeItems.Count;

	public static T Get()
	{
		if (!freeItems.TryDequeue(out var result))
		{
			return new T();
		}
		return result;
	}

	public static void Return(T item)
	{
		freeItems.Enqueue(item);
	}
}
