using System;

namespace Verse;

[Flags]
public enum LinkFlags
{
	None = 0,
	MapEdge = 1,
	Rock = 2,
	Wall = 4,
	Sandbags = 8,
	PowerConduit = 0x10,
	Barricades = 0x20,
	Fences = 0x40,
	Fleshmass = 0x80,
	SolidIce = 0x100,
	BurrowWall = 0x200,
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
