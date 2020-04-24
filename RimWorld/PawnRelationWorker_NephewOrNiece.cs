using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_NephewOrNiece : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			if (PawnRelationDefOf.Child.Worker.InRelation(me, other))
			{
				return false;
			}
			PawnRelationWorker worker = PawnRelationDefOf.Sibling.Worker;
			PawnRelationWorker worker2 = PawnRelationDefOf.HalfSibling.Worker;
			if ((other.GetMother() != null && (worker.InRelation(me, other.GetMother()) || worker2.InRelation(me, other.GetMother()))) || (other.GetFather() != null && (worker.InRelation(me, other.GetFather()) || worker2.InRelation(me, other.GetFather()))))
			{
				return true;
			}
			return false;
		}
	}
}
