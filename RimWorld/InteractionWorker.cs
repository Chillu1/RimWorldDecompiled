using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InteractionWorker
	{
		public InteractionDef interaction;

		public virtual float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			return 0f;
		}

		public virtual void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
		{
			letterText = null;
			letterLabel = null;
			letterDef = null;
			lookTargets = null;
		}
	}
}
