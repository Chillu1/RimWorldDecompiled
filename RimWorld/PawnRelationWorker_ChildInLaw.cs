using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_ChildInLaw : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			if (other.GetSpouse() == null)
			{
				return false;
			}
			PawnRelationWorker worker = PawnRelationDefOf.Child.Worker;
			if (worker.InRelation(me, other))
			{
				return false;
			}
			return worker.InRelation(me, other.GetSpouse());
		}
	}
}
