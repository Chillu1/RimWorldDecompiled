using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class InteractionWorker_InterrogateIdentity : InteractionWorker
{
	private const float MetalhorrorDetectionNoticed = 0.5f;

	public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
	{
		letterText = null;
		letterLabel = null;
		letterDef = null;
		lookTargets = null;
		if (Rand.Chance(InterrogateUtility.GetChance(initiator)))
		{
			TaggedString taggedString = "MetalhorrorReasonInterrogation".Translate(initiator.Named("PAWN"), recipient.Named("INFECTED"));
			TaggedString taggedString2 = "MetalhorrorNoticedDetailsAppended".Translate(initiator.Named("PAWN"), recipient.Named("INFECTED"));
			TaggedString taggedString3 = "InteractionDetectedDesc".Translate(initiator.Named("PAWN"), recipient.Named("INFECTED"));
			taggedString3 += $"\n\n{taggedString2}";
			MetalhorrorUtility.Detect(recipient, taggedString, taggedString3, 0.5f);
		}
	}
}
