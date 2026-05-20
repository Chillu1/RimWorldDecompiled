using Verse;

namespace RimWorld;

public class PawnRelationWorker_HalfSibling : PawnRelationWorker
{
	public override bool InRelation(Pawn me, Pawn other)
	{
		if (me == other)
		{
			return false;
		}
		if (PawnRelationDefOf.Sibling.Worker.InRelation(me, other))
		{
			return false;
		}
		if (me.HasSameMother(other) || me.HasSameFather(other))
		{
			return true;
		}
		return false;
	}
}
