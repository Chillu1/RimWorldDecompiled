using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace LudeonTK;

[NativeContainer]
[DebuggerDisplay("Count = {Count}, Capacity = {Capacity}, IsCreated = {IsCreated}")]
public struct NativePriorityQueue<TElement, TPriority, TComparer> : IDisposable where TElement : unmanaged where TPriority : unmanaged where TComparer : unmanaged, IComparer<TPriority>
{
	internal struct Data
	{
		public int m_Count;
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe void* m_ElementBuffer;

	[NativeDisableUnsafePtrRestriction]
	internal unsafe void* m_PriorityBuffer;

	[NativeDisableUnsafePtrRestriction]
	internal unsafe Data* m_Data;

	internal int m_Capacity;

	internal TComparer m_Comparer;

	internal Allocator m_AllocatorLabel;

	private const int Arity = 4;

	private const int Log2Arity = 2;

	public unsafe bool IsCreated => m_ElementBuffer != null;

	public int Capacity => m_Capacity;

	public unsafe int Count
	{
		get
		{
			if (m_Data == null)
			{
				return 0;
			}
			return m_Data->m_Count;
		}
	}

	public unsafe NativePriorityQueue(int capacity, TComparer comparer, Allocator allocator)
	{
		long size = (long)UnsafeUtility.SizeOf<TElement>() * (long)capacity;
		m_ElementBuffer = UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<TElement>(), allocator);
		long size2 = (long)UnsafeUtility.SizeOf<TPriority>() * (long)capacity;
		m_PriorityBuffer = UnsafeUtility.Malloc(size2, UnsafeUtility.AlignOf<TPriority>(), allocator);
		m_Data = (Data*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<Data>(), UnsafeUtility.AlignOf<Data>(), allocator);
		m_Data->m_Count = 0;
		m_Capacity = capacity;
		m_Comparer = comparer;
		m_AllocatorLabel = allocator;
	}

	[WriteAccessRequired]
	public unsafe void Clear()
	{
		m_Data->m_Count = 0;
	}

	[WriteAccessRequired]
	public unsafe void Enqueue(TElement element, TPriority priority)
	{
		int num = m_Data->m_Count;
		m_Data->m_Count = num + 1;
		TComparer comparer = m_Comparer;
		while (num > 0)
		{
			int num2 = num - 1 >> 2;
			TPriority val = UnsafeUtility.ReadArrayElement<TPriority>(m_PriorityBuffer, num2);
			if (comparer.Compare(priority, val) >= 0)
			{
				break;
			}
			TElement value = UnsafeUtility.ReadArrayElement<TElement>(m_ElementBuffer, num2);
			UnsafeUtility.WriteArrayElement(m_ElementBuffer, num, value);
			UnsafeUtility.WriteArrayElement(m_PriorityBuffer, num, val);
			num = num2;
		}
		UnsafeUtility.WriteArrayElement(m_ElementBuffer, num, element);
		UnsafeUtility.WriteArrayElement(m_PriorityBuffer, num, priority);
	}

	[WriteAccessRequired]
	public unsafe void Dequeue(out TElement element, out TPriority priority)
	{
		element = UnsafeUtility.ReadArrayElement<TElement>(m_ElementBuffer, 0);
		priority = UnsafeUtility.ReadArrayElement<TPriority>(m_PriorityBuffer, 0);
		RemoveRootNode();
	}

	private unsafe void RemoveRootNode()
	{
		int num = m_Data->m_Count - 1;
		m_Data->m_Count = num;
		if (num > 0)
		{
			TElement element = UnsafeUtility.ReadArrayElement<TElement>(m_ElementBuffer, num);
			TPriority priority = UnsafeUtility.ReadArrayElement<TPriority>(m_PriorityBuffer, num);
			MoveDown(element, priority, 0);
		}
	}

	private unsafe void MoveDown(TElement element, TPriority priority, int nodeIndex)
	{
		TComparer comparer = m_Comparer;
		int count = Count;
		int num;
		while ((num = (nodeIndex << 2) + 1) < count)
		{
			TElement value = UnsafeUtility.ReadArrayElement<TElement>(m_ElementBuffer, num);
			TPriority val = UnsafeUtility.ReadArrayElement<TPriority>(m_PriorityBuffer, num);
			int num2 = num;
			int num3 = math.min(num + 4, count);
			while (++num < num3)
			{
				TPriority val2 = UnsafeUtility.ReadArrayElement<TPriority>(m_PriorityBuffer, num);
				if (comparer.Compare(val2, val) < 0)
				{
					value = UnsafeUtility.ReadArrayElement<TElement>(m_ElementBuffer, num);
					val = val2;
					num2 = num;
				}
			}
			if (comparer.Compare(priority, val) <= 0)
			{
				break;
			}
			UnsafeUtility.WriteArrayElement(m_ElementBuffer, nodeIndex, value);
			UnsafeUtility.WriteArrayElement(m_PriorityBuffer, nodeIndex, val);
			nodeIndex = num2;
		}
		UnsafeUtility.WriteArrayElement(m_ElementBuffer, nodeIndex, element);
		UnsafeUtility.WriteArrayElement(m_PriorityBuffer, nodeIndex, priority);
	}

	[WriteAccessRequired]
	public unsafe void Dispose()
	{
		if (m_ElementBuffer == null)
		{
			throw new ObjectDisposedException("The collection is already disposed.");
		}
		if (m_AllocatorLabel == Allocator.Invalid)
		{
			throw new InvalidOperationException("The collection can not be Disposed because it was not allocated with a valid allocator.");
		}
		if (m_AllocatorLabel > Allocator.None)
		{
			UnsafeUtility.Free(m_ElementBuffer, m_AllocatorLabel);
			UnsafeUtility.Free(m_PriorityBuffer, m_AllocatorLabel);
			UnsafeUtility.Free(m_Data, m_AllocatorLabel);
			m_AllocatorLabel = Allocator.Invalid;
		}
		m_ElementBuffer = null;
		m_PriorityBuffer = null;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	internal static void CheckAllocateArguments(int capacity, Allocator allocator)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", "Capacity must be >= 0");
		}
		if (allocator <= Allocator.None || allocator > Allocator.Persistent)
		{
			throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", "allocator");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	internal static void CheckTotalSize(long totalSize)
	{
		if (totalSize > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("totalSize", $"Allocation total size cannot exceed {int.MaxValue} bytes");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	internal static void CheckNotEmpty(int count)
	{
		if (count == 0)
		{
			throw new InvalidOperationException("The container is empty");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	internal static void CheckSufficientCapacity(int capacity, int index)
	{
		if (index >= capacity)
		{
			throw new InvalidOperationException("The container has a fixed capacity and is already full.");
		}
	}
}
