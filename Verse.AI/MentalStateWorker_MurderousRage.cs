namespace Verse.AI
{
	public class MentalStateWorker_MurderousRage : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			return MurderousRageMentalStateUtility.FindPawnToKill(pawn) != null;
		}
	}
}
