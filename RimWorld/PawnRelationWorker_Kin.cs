using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class PawnRelationWorker_Kin : PawnRelationWorker
{
	public override bool InRelation(Pawn me, Pawn other)
	{
		if (me == other || me.RaceProps.Animal != other.RaceProps.Animal)
		{
			return false;
		}
		IEnumerable<Pawn> familyByBlood = me.relations.FamilyByBlood;
		if (!(familyByBlood is HashSet<Pawn> hashSet))
		{
			return familyByBlood.Contains(other);
		}
		return hashSet.Contains(other);
	}
}
