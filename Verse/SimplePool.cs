using System.Collections.Generic;

namespace Verse
{
	public static class SimplePool<T> where T : new()
	{
		private static List<T> freeItems = new List<T>();

		public static int FreeItemsCount => freeItems.Count;

		public static T Get()
		{
			if (freeItems.Count == 0)
			{
				return new T();
			}
			int index = freeItems.Count - 1;
			T result = freeItems[index];
			freeItems.RemoveAt(index);
			return result;
		}

		public static void Return(T item)
		{
			freeItems.Add(item);
		}
	}
}
