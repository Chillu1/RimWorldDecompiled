using System.Collections.Generic;
using UnityEngine;

namespace RimWorld
{
	public class Thought_MemorySocialCumulative : Thought_MemorySocial
	{
		private const float OpinionOffsetChangePerDay = 1f;

		public override bool ShouldDiscard => opinionOffset == 0f;

		public override float OpinionOffset()
		{
			if (ThoughtUtility.ThoughtNullified(pawn, def))
			{
				return 0f;
			}
			if (ShouldDiscard)
			{
				return 0f;
			}
			return Mathf.Min(opinionOffset, def.maxCumulatedOpinionOffset);
		}

		public override void ThoughtInterval()
		{
			base.ThoughtInterval();
			if (age < 60000)
			{
				return;
			}
			if (opinionOffset < 0f)
			{
				opinionOffset += 1f;
				if (opinionOffset > 0f)
				{
					opinionOffset = 0f;
				}
			}
			else if (opinionOffset > 0f)
			{
				opinionOffset -= 1f;
				if (opinionOffset < 0f)
				{
					opinionOffset = 0f;
				}
			}
			age = 0;
		}

		public override bool TryMergeWithExistingMemory(out bool showBubble)
		{
			showBubble = false;
			List<Thought_Memory> memories = pawn.needs.mood.thoughts.memories.Memories;
			for (int i = 0; i < memories.Count; i++)
			{
				if (memories[i].def == def)
				{
					Thought_MemorySocialCumulative thought_MemorySocialCumulative = (Thought_MemorySocialCumulative)memories[i];
					if (thought_MemorySocialCumulative.OtherPawn() == otherPawn)
					{
						thought_MemorySocialCumulative.opinionOffset += opinionOffset;
						return true;
					}
				}
			}
			return false;
		}
	}
}
