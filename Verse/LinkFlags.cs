using System;

namespace Verse
{
	[Flags]
	public enum LinkFlags
	{
		None = 0x0,
		MapEdge = 0x1,
		Rock = 0x2,
		Wall = 0x4,
		Sandbags = 0x8,
		PowerConduit = 0x10,
		Barricades = 0x20,
		Custom1 = 0x20000,
		Custom2 = 0x40000,
		Custom3 = 0x80000,
		Custom4 = 0x100000,
		Custom5 = 0x200000,
		Custom6 = 0x400000,
		Custom7 = 0x800000,
		Custom8 = 0x1000000,
		Custom9 = 0x2000000,
		Custom10 = 0x4000000
	}
}
