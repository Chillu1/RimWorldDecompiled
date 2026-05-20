using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class InteractionWorker_Nuzzle : InteractionWorker
{
	public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
	{
		AddNuzzledThought(initiator, recipient);
		letterText = null;
		letterLabel = null;
		letterDef = null;
		lookTargets = null;
	}

	private void AddNuzzledThought(Pawn initiator, Pawn recipient)
	{
		Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.Nuzzled);
		thought_Memory.SetForcedStage(NuzzleUtility.GetNuzzleStageIndex(initiator));
		if (recipient.needs.mood != null)
		{
			recipient.needs.mood.thoughts.memories.TryGainMemory(thought_Memory);
		}
	}
}
