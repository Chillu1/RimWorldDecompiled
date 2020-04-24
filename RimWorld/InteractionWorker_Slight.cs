using Verse;

namespace RimWorld
{
	public class InteractionWorker_Slight : InteractionWorker
	{
		private const float BaseSelectionWeight = 0.02f;

		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			return 0.02f * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient);
		}
	}
}
