using Verse;

namespace RimWorld
{
	public class InteractionWorker_Insult : InteractionWorker
	{
		private const float BaseSelectionWeight = 0.007f;

		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (initiator.IsSlave && !recipient.IsSlave)
			{
				return 0f;
			}
			return 0.007f * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient);
		}
	}
}
