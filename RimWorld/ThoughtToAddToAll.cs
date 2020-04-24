using System;
using Verse;

namespace RimWorld
{
	public struct ThoughtToAddToAll
	{
		public ThoughtDef thoughtDef;

		public Pawn otherPawn;

		public ThoughtToAddToAll(ThoughtDef thoughtDef, Pawn otherPawn = null)
		{
			if (thoughtDef == null)
			{
				throw new NullReferenceException("Thought def cant be null!");
			}
			this.thoughtDef = thoughtDef;
			this.otherPawn = otherPawn;
		}

		public void Add(Pawn to)
		{
			if (to.needs != null && to.needs.mood != null)
			{
				if (otherPawn == null)
				{
					to.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
				}
				else
				{
					to.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, otherPawn);
				}
			}
		}
	}
}
