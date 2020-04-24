using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_Nuzzle : InteractionWorker
	{
		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
		{
			AddNuzzledThought(initiator, recipient);
			TryGiveName(initiator, recipient);
			letterText = null;
			letterLabel = null;
			letterDef = null;
			lookTargets = null;
		}

		private void AddNuzzledThought(Pawn initiator, Pawn recipient)
		{
			Thought_Memory newThought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.Nuzzled);
			if (recipient.needs.mood != null)
			{
				recipient.needs.mood.thoughts.memories.TryGainMemory(newThought);
			}
		}

		private void TryGiveName(Pawn initiator, Pawn recipient)
		{
			if ((initiator.Name == null || initiator.Name.Numerical) && Rand.Value < initiator.RaceProps.nameOnNuzzleChance)
			{
				PawnUtility.GiveNameBecauseOfNuzzle(recipient, initiator);
			}
		}
	}
}
