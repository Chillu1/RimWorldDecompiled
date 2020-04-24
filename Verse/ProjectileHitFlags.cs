using System;

namespace Verse
{
	[Flags]
	public enum ProjectileHitFlags
	{
		None = 0x0,
		IntendedTarget = 0x1,
		NonTargetPawns = 0x2,
		NonTargetWorld = 0x4,
		All = -1
	}
}
