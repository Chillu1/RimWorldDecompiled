using System;

namespace RimWorld;

[Flags]
public enum CorridorShape : byte
{
	Straight = 1,
	Cross = 2,
	T = 4,
	H = 8,
	AsymmetricCross = 0x10,
	All = 0x1F
}
