using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Stepchild : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			if (me.GetSpouseCount(includeDead: true) == 0)
			{
				return false;
			}
			PawnRelationWorker worker = PawnRelationDefOf.Child.Worker;
			if (worker.InRelation(me, other))
			{
				return false;
			}
			foreach (Pawn spouse in me.GetSpouses(includeDead: true))
			{
				if (worker.InRelation(spouse, other))
				{
					return true;
				}
			}
			return false;
		}
	}
}
