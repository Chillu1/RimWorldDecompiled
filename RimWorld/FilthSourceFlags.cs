using System;

namespace RimWorld;

[Flags]
public enum FilthSourceFlags
{
	None = 0,
	Terrain = 1,
	Natural = 2,
	Unnatural = 4,
	Pawn = 8,
	Any = 0xF
}
