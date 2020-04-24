using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_ParentInLaw : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			if (me.GetSpouse() == null)
			{
				return false;
			}
			PawnRelationWorker worker = PawnRelationDefOf.Parent.Worker;
			if (worker.InRelation(me, other))
			{
				return false;
			}
			return worker.InRelation(me.GetSpouse(), other);
		}
	}
}
