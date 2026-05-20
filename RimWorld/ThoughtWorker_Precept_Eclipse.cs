using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_Eclipse : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!p.Spawned)
			{
				return false;
			}
			if (!p.Awake() || PawnUtility.IsBiologicallyOrArtificiallyBlind(p))
			{
				return false;
			}
			Room room = p.GetRoom();
			if (room != null && !room.PsychologicallyOutdoors)
			{
				return false;
			}
			return p.Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse);
		}
	}
}
