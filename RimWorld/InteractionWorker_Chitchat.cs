using Verse;

namespace RimWorld;

public class InteractionWorker_Chitchat : InteractionWorker
{
	public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
	{
		if (initiator.Inhumanized())
		{
			return 0f;
		}
		return 1f;
	}
}
