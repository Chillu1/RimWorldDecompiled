using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_GreatGrandparent : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			return PawnRelationDefOf.GreatGrandchild.Worker.InRelation(other, me);
		}
	}
}
