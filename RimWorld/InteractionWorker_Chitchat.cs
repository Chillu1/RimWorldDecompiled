using Verse;

namespace RimWorld
{
	public class InteractionWorker_Chitchat : InteractionWorker
	{
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			return 1f;
		}
	}
}
