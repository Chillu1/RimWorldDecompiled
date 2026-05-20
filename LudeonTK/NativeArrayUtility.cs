using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace LudeonTK;

public static class NativeArrayUtility
{
	private static class ArrayAccessor<T>
	{
		public static readonly Func<List<T>, T[]> Getter;

		static ArrayAccessor()
		{
			DynamicMethod dynamicMethod = new DynamicMethod("get", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(T[]), new Type[1] { typeof(List<T>) }, typeof(ArrayAccessor<T>), skipVisibility: true);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic));
			iLGenerator.Emit(OpCodes.Ret);
			Getter = (Func<List<T>, T[]>)dynamicMethod.CreateDelegate(typeof(Func<List<T>, T[]>));
		}
	}

	public static void EnsureDisposed<T>(this ref NativeArray<T> array) where T : unmanaged
	{
		if (array.IsCreated)
		{
			array.Dispose();
		}
	}

	public static void EnsureDisposed<T>(this ref NativeList<T> array) where T : unmanaged
	{
		if (array.IsCreated)
		{
			array.Dispose();
		}
	}

	public static void EnsureDisposed(this ref NativeBitArray array)
	{
		if (array.IsCreated)
		{
			array.Dispose();
		}
	}

	public static void EnsureDisposed<TElement, TPriority, TComparer>(this ref NativePriorityQueue<TElement, TPriority, TComparer> array) where TElement : unmanaged where TPriority : unmanaged where TComparer : unmanaged, IComparer<TPriority>
	{
		if (array.IsCreated)
		{
			array.Dispose();
		}
	}

	public static void EnsureDisposed<T>(this ref UnsafeList<T> list) where T : unmanaged
	{
		if (list.IsCreated)
		{
			list.Dispose();
		}
	}

	public static void EnsureDisposed<T, U>(this ref UnsafeHeap<T, U> list) where T : unmanaged where U : unmanaged, IComparer<T>
	{
		if (list.IsCreated)
		{
			list.Dispose();
		}
	}

	public static void EnsureDisposed(this ref UnsafeBitArray bitArray)
	{
		if (bitArray.IsCreated)
		{
			bitArray.Dispose();
		}
	}

	public unsafe static void MemClear<T>(NativeArray<T> array) where T : unmanaged
	{
		UnsafeUtility.MemClear(array.GetUnsafePtr(), (long)array.Length * (long)sizeof(T));
	}

	public unsafe static void MemClear<T>(UnsafeList<T> list) where T : unmanaged
	{
		UnsafeUtility.MemClear(list.Ptr, (long)list.Length * (long)sizeof(T));
	}

	public unsafe static void MemClear<T>(NativeList<T> list) where T : unmanaged
	{
		UnsafeUtility.MemClear(list.GetUnsafePtr(), (long)list.Length * (long)sizeof(T));
	}

	public static void Clear<T>(this NativeArray<T> array) where T : unmanaged
	{
		MemClear(array);
	}

	public static void Clear<T>(this NativeList<T> array) where T : unmanaged
	{
		MemClear(array);
	}

	public static void Clear<T>(this UnsafeList<T> array) where T : unmanaged
	{
		MemClear(array);
	}

	public static T[] GetInternalArray<T>(List<T> list)
	{
		return ArrayAccessor<T>.Getter(list);
	}

	public unsafe static NativeArray<T> GetNativeArrayCopy<T>(List<T> source, Allocator allocator, bool clearMemory = false) where T : unmanaged
	{
		T[] internalArray = GetInternalArray(source);
		NativeArray<T> nativeArray = new NativeArray<T>(source.Count, allocator, clearMemory ? NativeArrayOptions.ClearMemory : NativeArrayOptions.UninitializedMemory);
		fixed (T* ptr = internalArray)
		{
			void* source2 = ptr;
			UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray), source2, (long)source.Count * (long)UnsafeUtility.SizeOf<T>());
		}
		return nativeArray;
	}

	public unsafe static NativeArray<T> GetNativeArrayCopy<T>(T[] source, Allocator allocator, bool clearMemory = false) where T : unmanaged
	{
		NativeArray<T> nativeArray = new NativeArray<T>(source.Length, allocator, clearMemory ? NativeArrayOptions.ClearMemory : NativeArrayOptions.UninitializedMemory);
		fixed (T* ptr = source)
		{
			void* source2 = ptr;
			UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray), source2, (long)source.Length * (long)UnsafeUtility.SizeOf<T>());
		}
		return nativeArray;
	}

	public unsafe static void CopyNativeToArray<T>(T[] destination, NativeArray<T> source) where T : unmanaged
	{
		if (source.Length != destination.Length)
		{
			throw new Exception($"Attempted to copy a native array of length {source.Length} into an array with length {destination.Length}");
		}
		fixed (T* ptr = destination)
		{
			void* destination2 = ptr;
			UnsafeUtility.MemCpy(destination2, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(source), (long)destination.Length * (long)UnsafeUtility.SizeOf<T>());
		}
	}

	public unsafe static void CopyUnsafeListToArray<T>(T[] destination, UnsafeList<T> source) where T : unmanaged
	{
		if (source.Length != destination.Length)
		{
			throw new Exception($"Attempted to copy a unsafe array of length {source.Length} into an array with length {destination.Length}");
		}
		fixed (T* ptr = destination)
		{
			void* destination2 = ptr;
			UnsafeUtility.MemCpy(destination2, source.Ptr, (long)destination.Length * (long)UnsafeUtility.SizeOf<T>());
		}
	}

	public static void CopyNativeListToList<T>(List<T> destination, NativeList<T> source) where T : unmanaged
	{
		destination.Clear();
		destination.Capacity = Mathf.Max(source.Length, destination.Capacity);
		for (int i = 0; i < source.Length; i++)
		{
			destination.Add(source[i]);
		}
	}

	public static void CopyUnsafeListToList<T>(List<T> destination, UnsafeList<T> source) where T : unmanaged
	{
		destination.Clear();
		destination.Capacity = Mathf.Max(destination.Count, destination.Capacity);
		for (int i = 0; i < source.Length; i++)
		{
			destination.Add(source[i]);
		}
	}

	public static void CopyArrayToList<T>(List<T> destination, NativeArray<T> source) where T : unmanaged
	{
		destination.Clear();
		destination.Capacity = Mathf.Max(destination.Count, destination.Capacity);
		for (int i = 0; i < source.Length; i++)
		{
			destination.Add(source[i]);
		}
	}

	public unsafe static void CopyArrayToNative<T>(NativeArray<T> destination, T[] source) where T : unmanaged
	{
		if (source.Length != destination.Length)
		{
			throw new Exception($"Attempted to copy an array of length {source.Length} into a native array with length {destination.Length}");
		}
		fixed (T* ptr = source)
		{
			void* source2 = ptr;
			UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(destination), source2, (long)source.Length * (long)UnsafeUtility.SizeOf<T>());
		}
	}

	public static void CopyFrom(this NativeBitArray destination, NativeBitArray source)
	{
		destination.Copy(0, ref source, 0, Mathf.Min(destination.Length, source.Length));
	}

	public static void Reverse<T>(this NativeList<T> list) where T : unmanaged
	{
		int length = list.Length;
		int num = 0;
		int num2 = length - 1;
		while (num < num2)
		{
			int index = num;
			int index2 = num2;
			T val = list[num2];
			T val2 = list[num];
			T val3 = (list[index] = val);
			val3 = (list[index2] = val2);
			num++;
			num2--;
		}
	}

	public static NativeArray<T> EmptyArray<T>() where T : unmanaged
	{
		return new NativeArray<T>(0, Allocator.Persistent);
	}

	public static NativeList<T> EmptyList<T>() where T : unmanaged
	{
		return new NativeList<T>(0, Allocator.Persistent);
	}

	public static NativeBitArray EmptyBitArray()
	{
		return new NativeBitArray(0, Allocator.Persistent);
	}
}
