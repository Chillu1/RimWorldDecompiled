using RimWorld;

namespace Verse.AI
{
	public class MentalBreakWorker_FireTerror : MentalBreakWorker
	{
		public override bool BreakCanOccur(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return false;
			}
			if (!base.BreakCanOccur(pawn))
			{
				return false;
			}
			return ThoughtWorker_Pyrophobia.NearFire(pawn);
		}
	}
}
