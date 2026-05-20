using Verse;
using Verse.AI;

namespace RimWorld;

public class LordToil_ShamblerSwarm : LordToil_EntitySwarm
{
	public LordToil_ShamblerSwarm(IntVec3 start, IntVec3 dest)
		: base(start, dest)
	{
	}

	protected override DutyDef GetDutyDef()
	{
		return DutyDefOf.ShamblerSwarm;
	}

	public override bool CanAddPawn(Pawn p)
	{
		return p.IsShambler;
	}
}
