using System;

namespace Verse;

[Flags]
public enum RotEnum : byte
{
	North = 1,
	East = 2,
	South = 4,
	West = 8,
	All = 0xF
}
