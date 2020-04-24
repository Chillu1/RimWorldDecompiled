using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class InteractionWorker_SparkJailbreak : InteractionWorker
	{
		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
		{
			if (!recipient.IsPrisoner || !recipient.guest.PrisonerIsSecure || !PrisonBreakUtility.CanParticipateInPrisonBreak(recipient))
			{
				letterText = null;
				letterLabel = null;
				letterDef = null;
				lookTargets = null;
			}
			else
			{
				PrisonBreakUtility.StartPrisonBreak(recipient, out letterText, out letterLabel, out letterDef);
				lookTargets = new LookTargets(initiator, recipient);
				(initiator.MentalState as MentalState_Jailbreaker)?.Notify_InducedPrisonerToEscape();
			}
		}
	}
}
