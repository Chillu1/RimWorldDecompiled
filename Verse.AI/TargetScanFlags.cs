using System;

namespace Verse.AI;

[Flags]
public enum TargetScanFlags
{
	None = 0,
	NeedLOSToPawns = 1,
	NeedLOSToNonPawns = 2,
	NeedLOSToAll = 3,
	NeedReachable = 4,
	NeedReachableIfCantHitFromMyPos = 8,
	NeedNonBurning = 0x10,
	NeedThreat = 0x20,
	NeedActiveThreat = 0x40,
	LOSBlockableByGas = 0x80,
	NeedAutoTargetable = 0x100,
	NeedNotUnderThickRoof = 0x200,
	IgnoreNonCombatants = 0x400
}
