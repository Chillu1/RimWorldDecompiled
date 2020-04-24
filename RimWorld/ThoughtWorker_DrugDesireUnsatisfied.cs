using Verse;

namespace RimWorld
{
	public class ThoughtWorker_DrugDesireUnsatisfied : ThoughtWorker
	{
		private const int Neutral = 3;

		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			Need_Chemical_Any drugsDesire = p.needs.drugsDesire;
			if (drugsDesire == null)
			{
				return false;
			}
			int moodBuffForCurrentLevel = (int)drugsDesire.MoodBuffForCurrentLevel;
			if (moodBuffForCurrentLevel < 3)
			{
				return ThoughtState.ActiveAtStage(3 - moodBuffForCurrentLevel - 1);
			}
			return false;
		}
	}
}
