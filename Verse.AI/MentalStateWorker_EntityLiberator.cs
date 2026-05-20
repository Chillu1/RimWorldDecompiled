using RimWorld;

namespace Verse.AI;

public class MentalStateWorker_EntityLiberator : MentalStateWorker
{
	public override bool StateCanOccur(Pawn pawn)
	{
		if (!base.StateCanOccur(pawn))
		{
			return false;
		}
		return AnomalyUtility.FindEntityOnPlatform(pawn.Map, EntityQueryType.ForRelease) != null;
	}
}
