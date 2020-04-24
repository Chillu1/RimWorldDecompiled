using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalStateWorker_SadisticRageTantrum : MentalStateWorker
	{
		private static List<Thing> tmpThings = new List<Thing>();

		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			tmpThings.Clear();
			TantrumMentalStateUtility.GetSmashableThingsNear(pawn, pawn.Position, tmpThings, (Thing x) => TantrumMentalStateUtility.CanAttackPrisoner(pawn, x));
			bool result = tmpThings.Any();
			tmpThings.Clear();
			return result;
		}
	}
}
