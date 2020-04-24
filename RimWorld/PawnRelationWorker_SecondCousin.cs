using Verse;

namespace RimWorld
{
	public class PawnRelationWorker_SecondCousin : PawnRelationWorker
	{
		public override bool InRelation(Pawn me, Pawn other)
		{
			if (me == other)
			{
				return false;
			}
			PawnRelationWorker worker = PawnRelationDefOf.GranduncleOrGrandaunt.Worker;
			Pawn mother = other.GetMother();
			if (mother != null && ((mother.GetMother() != null && worker.InRelation(me, mother.GetMother())) || (mother.GetFather() != null && worker.InRelation(me, mother.GetFather()))))
			{
				return true;
			}
			Pawn father = other.GetFather();
			if (father != null && ((father.GetMother() != null && worker.InRelation(me, father.GetMother())) || (father.GetFather() != null && worker.InRelation(me, father.GetFather()))))
			{
				return true;
			}
			return false;
		}
	}
}
