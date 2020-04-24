using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_WantToSleepWithSpouseOrLover : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(p, allowDead: false);
			if (directPawnRelation == null)
			{
				return false;
			}
			if (!directPawnRelation.otherPawn.IsColonist || directPawnRelation.otherPawn.IsWorldPawn() || !directPawnRelation.otherPawn.relations.everSeenByPlayer)
			{
				return false;
			}
			if (p.ownership.OwnedBed != null && p.ownership.OwnedBed == directPawnRelation.otherPawn.ownership.OwnedBed)
			{
				return false;
			}
			if (p.relations.OpinionOf(directPawnRelation.otherPawn) <= 0)
			{
				return false;
			}
			return true;
		}
	}
}
