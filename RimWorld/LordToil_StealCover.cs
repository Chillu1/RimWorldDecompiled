using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class LordToil_StealCover : LordToil_DoOpportunisticTaskOrCover
	{
		protected override DutyDef DutyDef => DutyDefOf.Steal;

		public override bool ForceHighStoryDanger => true;

		public override bool AllowSelfTend => false;

		protected override bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets)
		{
			if (pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDef && pawn.carryTracker.CarriedThing != null)
			{
				target = pawn.carryTracker.CarriedThing;
				return true;
			}
			return StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 7f, out target, pawn, alreadyTakenTargets);
		}
	}
}
