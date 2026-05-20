using RimWorld;

namespace Verse.AI
{
	public class JobGiver_WanderInPen : JobGiver_Wander
	{
		public JobGiver_WanderInPen()
		{
			wanderRadius = 10f;
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			if (pawn.GetDistrict().TouchesMapEdge)
			{
				CompAnimalPenMarker compAnimalPenMarker = AnimalPenUtility.ClosestSuitablePen(pawn, allowUnenclosedPens: true);
				if (compAnimalPenMarker != null)
				{
					return compAnimalPenMarker.parent.Position;
				}
			}
			return pawn.Position;
		}
	}
}
