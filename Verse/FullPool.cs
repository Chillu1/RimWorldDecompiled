using System.Collections.Generic;

namespace Verse
{
	public static class FullPool<T> where T : IFullPoolable, new()
	{
		private static List<T> freeItems = new List<T>();

		public static T Get()
		{
			if (freeItems.Count == 0)
			{
				return new T();
			}
			T result = freeItems[freeItems.Count - 1];
			freeItems.RemoveAt(freeItems.Count - 1);
			return result;
		}

		public static void Return(T item)
		{
			item.Reset();
			freeItems.Add(item);
		}
	}
}
