using System;

namespace RimWorld;

[Flags]
public enum SpectateRectSide
{
	None = 0,
	Up = 1,
	Right = 2,
	Down = 4,
	Left = 8,
	Vertical = 5,
	Horizontal = 0xA,
	All = 0xF
}
