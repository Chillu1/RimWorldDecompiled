using Verse;

namespace RimWorld
{
	public class InteractionWorker_Slight : InteractionWorker
	{
		private const float BaseSelectionWeight = 0.02f;

		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (initiator.IsSlave && !recipient.IsSlave)
			{
				return 0f;
			}
			return 0.02f * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient);
		}
	}
}
