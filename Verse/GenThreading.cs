using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Verse;

public static class GenThreading
{
	public struct Slice
	{
		public readonly int fromInclusive;

		public readonly int toExclusive;

		public Slice(int fromInclusive, int toExclusive)
		{
			this.fromInclusive = fromInclusive;
			this.toExclusive = toExclusive;
		}
	}

	public static int ProcessorCount => Environment.ProcessorCount;

	private static void GetMaxDegreeOfParallelism(ref int maxDegreeOfParallelism)
	{
		if (maxDegreeOfParallelism == -1)
		{
			maxDegreeOfParallelism = ProcessorCount;
		}
	}

	public static void SliceWorkNoAlloc(int fromInclusive, int toExclusive, int maxBatches, List<Slice> batches)
	{
		int num = toExclusive - fromInclusive;
		if (num <= 0)
		{
			return;
		}
		int num2 = num % maxBatches;
		int num3 = Mathf.FloorToInt((float)num / (float)maxBatches);
		if (num3 > 0)
		{
			int num4 = 0;
			for (int i = 0; i < maxBatches; i++)
			{
				int num5 = num3;
				if (num2 > 0)
				{
					num5++;
					num2--;
				}
				batches.Add(new Slice(num4, num4 + num5));
				num4 += num5;
			}
		}
		else
		{
			for (int j = 0; j < num2; j++)
			{
				batches.Add(new Slice(j, j + 1));
			}
		}
	}

	public static List<Slice> SliceWork(int fromInclusive, int toExclusive, int maxBatches)
	{
		List<Slice> list = new List<Slice>(maxBatches);
		SliceWorkNoAlloc(fromInclusive, toExclusive, maxBatches, list);
		return list;
	}

	public static List<List<T>> SliceWork<T>(List<T> list, int maxBatches)
	{
		List<List<T>> list2 = new List<List<T>>(maxBatches);
		foreach (Slice item in SliceWork(0, list.Count, maxBatches))
		{
			List<T> list3 = new List<T>(item.toExclusive - item.fromInclusive);
			for (int i = item.fromInclusive; i < item.toExclusive; i++)
			{
				list3.Add(list[i]);
			}
			list2.Add(list3);
		}
		return list2;
	}

	public static void ParallelForEach<T>(List<T> list, Action<T> callback, int maxDegreeOfParallelism = -1)
	{
		GetMaxDegreeOfParallelism(ref maxDegreeOfParallelism);
		int count = list.Count;
		long tasksDone = 0L;
		AutoResetEvent taskDoneEvent = new AutoResetEvent(initialState: false);
		foreach (List<T> item in SliceWork(list, maxDegreeOfParallelism))
		{
			List<T> localBatch = item;
			ThreadPool.QueueUserWorkItem(delegate
			{
				foreach (T item2 in localBatch)
				{
					try
					{
						callback(item2);
					}
					catch (Exception ex)
					{
						Log.Error("Error in ParallelForEach(): " + ex);
					}
				}
				Interlocked.Add(ref tasksDone, localBatch.Count);
				taskDoneEvent.Set();
			});
		}
		while (Interlocked.Read(ref tasksDone) < count)
		{
			taskDoneEvent.WaitOne();
		}
	}

	public static void ParallelFor(int fromInclusive, int toExclusive, Action<int> callback, int maxDegreeOfParallelism = -1)
	{
		GetMaxDegreeOfParallelism(ref maxDegreeOfParallelism);
		int num = toExclusive - fromInclusive;
		long tasksDone = 0L;
		AutoResetEvent taskDoneEvent = new AutoResetEvent(initialState: false);
		foreach (Slice item in SliceWork(fromInclusive, toExclusive, maxDegreeOfParallelism))
		{
			Slice localBatch = item;
			ThreadPool.QueueUserWorkItem(delegate
			{
				for (int i = localBatch.fromInclusive; i < localBatch.toExclusive; i++)
				{
					try
					{
						callback(i);
					}
					catch (Exception ex)
					{
						Log.Error("Error in ParallelFor(): " + ex);
					}
				}
				Interlocked.Add(ref tasksDone, localBatch.toExclusive - localBatch.fromInclusive);
				taskDoneEvent.Set();
			});
		}
		while (Interlocked.Read(ref tasksDone) < num)
		{
			taskDoneEvent.WaitOne();
		}
	}
}
