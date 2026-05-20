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
			if (other.GetSpouseCount(includeDead: true) == 0)
			{
				return false;
			}
			PawnRelationWorker worker = PawnRelationDefOf.Child.Worker;
			if (worker.InRelation(me, other))
			{
				return false;
			}
			foreach (Pawn spouse in other.GetSpouses(includeDead: true))
			{
				if (worker.InRelation(me, spouse))
				{
					return true;
				}
			}
			return false;
		}
	}
}
