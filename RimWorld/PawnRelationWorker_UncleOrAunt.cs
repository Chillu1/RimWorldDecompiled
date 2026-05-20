using Verse;

namespace RimWorld;

public class PawnRelationWorker_UncleOrAunt : PawnRelationWorker
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
		_ = PawnRelationDefOf.Grandparent.Worker;
		if ((me.GetMother() != null && (other.HasSameMother(me.GetMother()) || other.HasSameFather(me.GetMother()))) || (me.GetFather() != null && (other.HasSameMother(me.GetFather()) || other.HasSameFather(me.GetFather()))))
		{
			return true;
		}
		return false;
	}
}
