using Verse;

namespace RimWorld
{
	public class RitualBehaviorWorker_PrisonerSacrifice : RitualBehaviorWorker
	{
		public RitualBehaviorWorker_PrisonerSacrifice()
		{
		}

		public RitualBehaviorWorker_PrisonerSacrifice(RitualBehaviorDef def)
			: base(def)
		{
		}

		public override void PostCleanup(LordJob_Ritual ritual)
		{
			Pawn warden = ritual.PawnWithRole("moralist");
			Pawn pawn = ritual.PawnWithRole("prisoner");
			if (pawn.IsPrisonerOfColony)
			{
				WorkGiver_Warden_TakeToBed.TryTakePrisonerToBed(pawn, warden);
				pawn.guest.WaitInsteadOfEscapingFor(1250);
			}
		}
	}
}
