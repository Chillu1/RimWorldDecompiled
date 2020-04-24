using System;
using UnityEngine;

namespace Verse
{
	internal class Deque<T>
	{
		private T[] data;

		private int first;

		private int count;

		public bool Empty => count == 0;

		public Deque()
		{
			data = new T[8];
			first = 0;
			count = 0;
		}

		public void PushFront(T item)
		{
			PushPrep();
			first--;
			if (first < 0)
			{
				first += data.Length;
			}
			count++;
			data[first] = item;
		}

		public void PushBack(T item)
		{
			PushPrep();
			data[(first + count++) % data.Length] = item;
		}

		public T PopFront()
		{
			T result = data[first];
			data[first] = default(T);
			first = (first + 1) % data.Length;
			count--;
			return result;
		}

		public void Clear()
		{
			first = 0;
			count = 0;
		}

		private void PushPrep()
		{
			if (count >= data.Length)
			{
				T[] destinationArray = new T[data.Length * 2];
				Array.Copy(data, first, destinationArray, 0, Mathf.Min(count, data.Length - first));
				if (first + count > data.Length)
				{
					Array.Copy(data, 0, destinationArray, data.Length - first, count - data.Length + first);
				}
				data = destinationArray;
				first = 0;
			}
		}
	}
}
