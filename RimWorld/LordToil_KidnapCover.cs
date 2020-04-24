using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class LordToil_KidnapCover : LordToil_DoOpportunisticTaskOrCover
	{
		protected override DutyDef DutyDef => DutyDefOf.Kidnap;

		public override bool ForceHighStoryDanger => cover;

		public override bool AllowSelfTend => false;

		protected override bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets)
		{
			if (pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDef && pawn.carryTracker.CarriedThing is Pawn)
			{
				target = pawn.carryTracker.CarriedThing;
				return true;
			}
			Pawn victim;
			bool result = KidnapAIUtility.TryFindGoodKidnapVictim(pawn, 8f, out victim, alreadyTakenTargets);
			target = victim;
			return result;
		}
	}
}
