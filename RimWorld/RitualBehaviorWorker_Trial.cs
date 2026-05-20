using Verse;

namespace RimWorld
{
	public class RitualBehaviorWorker_Trial : RitualBehaviorWorker
	{
		private int ticksSinceLastInteraction = -1;

		public const int SocialInteractionIntervalTicks = 700;

		public RitualBehaviorWorker_Trial()
		{
		}

		public RitualBehaviorWorker_Trial(RitualBehaviorDef def)
			: base(def)
		{
		}

		public override void Cleanup(LordJob_Ritual ritual)
		{
			Pawn pawn = ritual.PawnWithRole("convict");
			if (pawn.IsPrisonerOfColony)
			{
				pawn.guest.WaitInsteadOfEscapingFor(2500);
			}
		}

		public override void PostCleanup(LordJob_Ritual ritual)
		{
			Pawn warden = ritual.PawnWithRole("leader");
			Pawn pawn = ritual.PawnWithRole("convict");
			if (pawn.IsPrisonerOfColony)
			{
				WorkGiver_Warden_TakeToBed.TryTakePrisonerToBed(pawn, warden);
				pawn.guest.WaitInsteadOfEscapingFor(1250);
			}
		}

		public override void Tick(LordJob_Ritual ritual)
		{
			base.Tick(ritual);
			if (ritual.StageIndex == 0)
			{
				return;
			}
			if (ticksSinceLastInteraction == -1 || ticksSinceLastInteraction > 700)
			{
				ticksSinceLastInteraction = 0;
				Pawn pawn = ritual.PawnWithRole("leader");
				Pawn pawn2 = ritual.PawnWithRole("convict");
				if (Rand.Bool)
				{
					pawn.interactions.TryInteractWith(pawn2, InteractionDefOf.Trial_Accuse);
				}
				else
				{
					pawn2.interactions.TryInteractWith(pawn, InteractionDefOf.Trial_Defend);
				}
			}
			else
			{
				ticksSinceLastInteraction++;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksSinceLastInteraction, "ticksSinceLastInteraction", -1);
		}
	}
}
