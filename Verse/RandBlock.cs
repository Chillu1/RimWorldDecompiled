using System;
using System.Runtime.InteropServices;

namespace Verse;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RandBlock : IDisposable
{
	public RandBlock(int seed)
	{
		Rand.PushState(seed);
	}

	public void Dispose()
	{
		Rand.PopState();
	}

	public static RandBlock Seed(int seed)
	{
		return new RandBlock(seed);
	}
}
