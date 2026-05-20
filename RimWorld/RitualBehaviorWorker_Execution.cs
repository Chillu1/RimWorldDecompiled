using Verse;

namespace RimWorld;

public class RitualBehaviorWorker_Execution : RitualBehaviorWorker
{
	public RitualBehaviorWorker_Execution()
	{
	}

	public RitualBehaviorWorker_Execution(RitualBehaviorDef def)
		: base(def)
	{
	}

	public override void PostCleanup(LordJob_Ritual ritual)
	{
		Pawn warden = ritual.PawnWithRole("executioner");
		Pawn pawn = ritual.PawnWithRole("prisoner");
		if (pawn != null && pawn.IsPrisonerOfColony)
		{
			WorkGiver_Warden_TakeToBed.TryTakePrisonerToBed(pawn, warden);
			pawn.guest.WaitInsteadOfEscapingFor(1250);
		}
	}
}
