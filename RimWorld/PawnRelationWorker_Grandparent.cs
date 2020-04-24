using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Grandparent : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			return PawnRelationDefOf.Grandchild.Worker.InRelation(other, me);
		}
	}
}
