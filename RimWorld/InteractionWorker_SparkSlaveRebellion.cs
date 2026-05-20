using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_SparkSlaveRebellion : InteractionWorker
	{
		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
		{
			if (!SlaveRebellionUtility.CanParticipateInSlaveRebellion(recipient))
			{
				letterText = null;
				letterLabel = null;
				letterDef = null;
				lookTargets = null;
			}
			else
			{
				SlaveRebellionUtility.StartSlaveRebellion(recipient, out letterText, out letterLabel, out letterDef, out lookTargets);
				lookTargets = new LookTargets(initiator, recipient);
			}
		}
	}
}
