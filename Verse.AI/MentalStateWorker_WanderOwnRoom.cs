using RimWorld;

namespace Verse.AI
{
	public class MentalStateWorker_WanderOwnRoom : MentalStateWorker
	{
		public override bool StateCanOccur(Pawn pawn)
		{
			if (!base.StateCanOccur(pawn))
			{
				return false;
			}
			Building_Bed ownedBed = pawn.ownership.OwnedBed;
			if (ownedBed == null || ownedBed.GetRoom() == null || ownedBed.GetRoom().PsychologicallyOutdoors)
			{
				return false;
			}
			return true;
		}
	}
}
