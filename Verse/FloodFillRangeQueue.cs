using System;

namespace Verse
{
	public class FloodFillRangeQueue
	{
		private FloodFillRange[] array;

		private int count;

		private int head;

		private int debugNumTimesExpanded;

		private int debugMaxUsedSpace;

		public int Count => count;

		public FloodFillRange First => array[head];

		public string PerfDebugString => "NumTimesExpanded: " + debugNumTimesExpanded + ", MaxUsedSize= " + debugMaxUsedSpace + ", ClaimedSize=" + array.Length + ", UnusedSpace=" + (array.Length - debugMaxUsedSpace);

		public FloodFillRangeQueue(int initialSize)
		{
			array = new FloodFillRange[initialSize];
			head = 0;
			count = 0;
		}

		public void Enqueue(FloodFillRange r)
		{
			if (count + head == array.Length)
			{
				FloodFillRange[] destinationArray = new FloodFillRange[2 * array.Length];
				Array.Copy(array, head, destinationArray, 0, count);
				array = destinationArray;
				head = 0;
				debugNumTimesExpanded++;
			}
			array[head + count++] = r;
			debugMaxUsedSpace = count + head;
		}

		public FloodFillRange Dequeue()
		{
			FloodFillRange result = default(FloodFillRange);
			if (count > 0)
			{
				result = array[head];
				array[head] = default(FloodFillRange);
				head++;
				count--;
			}
			return result;
		}
	}
}
