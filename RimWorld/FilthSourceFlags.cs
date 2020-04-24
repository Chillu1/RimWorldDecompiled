using System;

namespace RimWorld
{
	[Flags]
	public enum FilthSourceFlags
	{
		None = 0x0,
		Terrain = 0x1,
		Natural = 0x2,
		Unnatural = 0x4,
		Any = 0x7
	}
}
