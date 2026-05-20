using System;
using LudeonTK;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Verse.Glow;

public struct LocalGlowArea : IDisposable
{
	public bool inUse;

	public UnsafeList<Color32> colors;

	public static LocalGlowArea AllocateNew()
	{
		LocalGlowArea result = new LocalGlowArea
		{
			inUse = false,
			colors = new UnsafeList<Color32>(6561, Allocator.Persistent, NativeArrayOptions.ClearMemory)
		};
		result.colors.Resize(6561, NativeArrayOptions.ClearMemory);
		return result;
	}

	public void Take()
	{
		inUse = true;
	}

	public void Return()
	{
		NativeArrayUtility.MemClear(colors);
		inUse = false;
	}

	public void Dispose()
	{
		NativeArrayUtility.EnsureDisposed(ref colors);
	}
}
