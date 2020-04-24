namespace Verse.AI
{
	public class MentalStateWorker_Slaughterer : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			return SlaughtererMentalStateUtility.FindAnimal(pawn) != null;
		}
	}
}
