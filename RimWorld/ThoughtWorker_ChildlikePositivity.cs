using Verse;

namespace RimWorld
{
	public class ThoughtWorker_ChildlikePositivity : ThoughtWorker
	{
		private const int MinAge = 3;

		private static readonly int[] AgeThresholds = new int[4] { 6, 8, 10, 13 };

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!p.IsFreeColonist || p.Suspended || p.DevelopmentalStage.Adult())
			{
				return ThoughtState.Inactive;
			}
			float ageBiologicalYearsFloat = p.ageTracker.AgeBiologicalYearsFloat;
			if (ageBiologicalYearsFloat < 3f)
			{
				return ThoughtState.Inactive;
			}
			for (int i = 0; i < AgeThresholds.Length; i++)
			{
				if (ageBiologicalYearsFloat <= (float)AgeThresholds[i])
				{
					return ThoughtState.ActiveAtStage(i);
				}
			}
			return ThoughtState.Inactive;
		}
	}
}
