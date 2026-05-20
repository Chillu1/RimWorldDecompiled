using System;

namespace Verse;

[Flags]
public enum RotDrawMode : byte
{
	Fresh = 1,
	Rotting = 2,
	Dessicated = 4
}
