using System.Collections.Generic;

namespace Verse.AI
{
	public class MentalStateWorker_TantrumAll : MentalStateWorker
	{
		private static List<Thing> tmpThings = new List<Thing>();

		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			tmpThings.Clear();
			TantrumMentalStateUtility.GetSmashableThingsNear(pawn, pawn.Position, tmpThings);
			bool result = tmpThings.Count >= 2;
			tmpThings.Clear();
			return result;
		}
	}
}
