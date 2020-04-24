using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalStateWorker_TargetedInsultingSpree : MentalStateWorker
	{
		private static List<Pawn> candidates = new List<Pawn>();

		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(pawn, candidates, allowPrisoners: false);
			bool result = candidates.Any();
			candidates.Clear();
			return result;
		}
	}
}
