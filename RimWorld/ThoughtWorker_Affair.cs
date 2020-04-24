using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Affair : ThoughtWorker
	{
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
		{
			if (!p.relations.DirectRelationExists(PawnRelationDefOf.Spouse, otherPawn))
			{
				return false;
			}
			List<DirectPawnRelation> directRelations = otherPawn.relations.DirectRelations;
			for (int i = 0; i < directRelations.Count; i++)
			{
				if (directRelations[i].otherPawn != p && !directRelations[i].otherPawn.Dead && (directRelations[i].def == PawnRelationDefOf.Lover || directRelations[i].def == PawnRelationDefOf.Fiance))
				{
					return true;
				}
			}
			return false;
		}
	}
}
