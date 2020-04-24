using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalStateWorker_InsultingSpreeAll : MentalStateWorker
	{
		private static List<Pawn> candidates = new List<Pawn>();

		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			InsultingSpreeMentalStateUtility.GetInsultCandidatesFor(pawn, candidates);
			bool result = candidates.Count >= 2;
			candidates.Clear();
			return result;
		}
	}
}
