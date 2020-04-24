using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_MarryAdjacentPawn : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return null;
			}
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = pawn.Position + GenAdj.CardinalDirections[i];
				if (c.InBounds(pawn.Map))
				{
					Thing thing = c.GetThingList(pawn.Map).Find((Thing x) => x is Pawn && CanMarry(pawn, (Pawn)x));
					if (thing != null)
					{
						return JobMaker.MakeJob(JobDefOf.MarryAdjacentPawn, thing);
					}
				}
			}
			return null;
		}

		private bool CanMarry(Pawn pawn, Pawn toMarry)
		{
			if (!toMarry.Drafted)
			{
				return pawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, toMarry);
			}
			return false;
		}
	}
}
