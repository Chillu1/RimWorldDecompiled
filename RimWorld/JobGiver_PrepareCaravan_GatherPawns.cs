using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	[Obsolete]
	public class JobGiver_PrepareCaravan_GatherPawns : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			return null;
		}

		private Pawn FindPawn(Pawn pawn)
		{
			return null;
		}
	}
}
