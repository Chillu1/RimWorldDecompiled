using System.Collections.Generic;

namespace Verse
{
	public class SimpleLinearPool<T> where T : new()
	{
		private List<T> items = new List<T>();

		private int readIndex;

		public T Get()
		{
			if (readIndex >= items.Count)
			{
				items.Add(new T());
			}
			return items[readIndex++];
		}

		public void Clear()
		{
			readIndex = 0;
		}
	}
}
