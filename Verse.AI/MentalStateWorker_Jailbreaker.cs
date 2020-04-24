using RimWorld;

namespace Verse.AI
{
	public class MentalStateWorker_Jailbreaker : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
			{
				return false;
			}
			return JailbreakerMentalStateUtility.FindPrisoner(pawn) != null;
		}
	}
}
