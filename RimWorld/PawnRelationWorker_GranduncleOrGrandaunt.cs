using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_GranduncleOrGrandaunt : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			if (PawnRelationDefOf.Grandparent.Worker.InRelation(me, other))
			{
				return false;
			}
			PawnRelationWorker worker = PawnRelationDefOf.GreatGrandparent.Worker;
			if ((other.GetMother() != null && worker.InRelation(me, other.GetMother())) || (other.GetFather() != null && worker.InRelation(me, other.GetFather())))
			{
				return true;
			}
			return false;
		}
	}
}
