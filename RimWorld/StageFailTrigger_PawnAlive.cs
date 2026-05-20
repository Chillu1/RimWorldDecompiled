using Verse;

namespace RimWorld;

public class StageFailTrigger_PawnAlive : StageFailTrigger
{
	[NoTranslate]
	public string pawnId;

	public override bool Failed(LordJob_Ritual ritual, TargetInfo spot, TargetInfo focus)
	{
		Pawn pawn = ritual.PawnWithRole(pawnId);
		if (pawn == null)
		{
			return false;
		}
		return !pawn.Dead;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref pawnId, "pawnId");
	}
}
