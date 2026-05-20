using System;

namespace Verse;

[Flags]
public enum ProjectileHitFlags
{
	None = 0,
	IntendedTarget = 1,
	NonTargetPawns = 2,
	NonTargetWorld = 4,
	All = -1
}
