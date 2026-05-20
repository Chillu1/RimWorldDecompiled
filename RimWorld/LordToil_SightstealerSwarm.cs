using Verse;
using Verse.AI;

namespace RimWorld;

public class LordToil_SightstealerSwarm : LordToil_EntitySwarm
{
	public LordToil_SightstealerSwarm(IntVec3 start, IntVec3 dest)
		: base(start, dest)
	{
	}

	protected override DutyDef GetDutyDef()
	{
		return DutyDefOf.SightstealerSwarm;
	}

	public override bool CanAddPawn(Pawn p)
	{
		return p.kindDef == PawnKindDefOf.Sightstealer;
	}
}
