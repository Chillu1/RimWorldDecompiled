using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_Suppress : InteractionWorker
	{
		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
		{
			letterText = null;
			letterLabel = null;
			letterDef = null;
			lookTargets = null;
			SlaveRebellionUtility.IncrementInteractionSuppression(initiator, recipient);
		}
	}
}
