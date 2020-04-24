using UnityEngine;
using Verse;

namespace RimWorld
{
	public struct IndividualThoughtToAdd
	{
		public Thought_Memory thought;

		public Pawn addTo;

		private Pawn otherPawn;

		public string LabelCap
		{
			get
			{
				string text = thought.LabelCap;
				float num = thought.MoodOffset();
				if (num != 0f)
				{
					text = text + " " + Mathf.RoundToInt(num).ToStringWithSign();
				}
				return text;
			}
		}

		public IndividualThoughtToAdd(ThoughtDef thoughtDef, Pawn addTo, Pawn otherPawn = null, float moodPowerFactor = 1f, float opinionOffsetFactor = 1f)
		{
			this.addTo = addTo;
			this.otherPawn = otherPawn;
			thought = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
			thought.moodPowerFactor = moodPowerFactor;
			thought.otherPawn = otherPawn;
			thought.pawn = addTo;
			Thought_MemorySocial thought_MemorySocial = thought as Thought_MemorySocial;
			if (thought_MemorySocial != null)
			{
				thought_MemorySocial.opinionOffset *= opinionOffsetFactor;
			}
		}

		public void Add()
		{
			if (addTo.needs != null && addTo.needs.mood != null)
			{
				addTo.needs.mood.thoughts.memories.TryGainMemory(thought, otherPawn);
			}
		}
	}
}
