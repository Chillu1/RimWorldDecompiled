using System;

namespace Verse;

[Flags]
public enum LinkDirections : byte
{
	None = 0,
	Up = 1,
	Right = 2,
	Down = 4,
	Left = 8
}
