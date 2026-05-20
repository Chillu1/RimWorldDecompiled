using Verse;

namespace RimWorld;

public class Building_TrapReleaseWasp : Building_TrapReleaseEntity
{
	protected override int CountToSpawn => 3;

	protected override PawnKindDef PawnToSpawn => PawnKindDefOf.Drone_Wasp;
}
