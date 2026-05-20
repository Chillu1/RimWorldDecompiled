namespace Verse.AI
{
	public class MentalStateWorker_RitualBlinding : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			return RitualBlindingMentalStateUtility.FindPawnToBlind(pawn) != null;
		}
	}
}
