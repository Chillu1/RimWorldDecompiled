using Verse;

namespace RimWorld;

public class Building_TrapReleaseHunter : Building_TrapReleaseEntity
{
	protected override int CountToSpawn => 1;

	protected override PawnKindDef PawnToSpawn => PawnKindDefOf.Drone_Hunter;
}
