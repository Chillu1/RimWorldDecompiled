using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_Stepparent : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			if (PawnRelationDefOf.Parent.Worker.InRelation(me, other))
			{
				return false;
			}
			PawnRelationWorker worker = PawnRelationDefOf.Spouse.Worker;
			if (me.GetMother() != null && worker.InRelation(me.GetMother(), other))
			{
				return true;
			}
			if (me.GetFather() != null && worker.InRelation(me.GetFather(), other))
			{
				return true;
			}
			return false;
		}
	}
}
