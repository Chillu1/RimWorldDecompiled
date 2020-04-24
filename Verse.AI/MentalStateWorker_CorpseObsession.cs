namespace Verse.AI
{
	public class MentalStateWorker_CorpseObsession : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			return CorpseObsessionMentalStateUtility.GetClosestCorpseToDigUp(pawn) != null;
		}
	}
}
