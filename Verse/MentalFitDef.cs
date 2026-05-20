using System.Collections.Generic;

namespace Verse
{
	public class MentalFitDef : Def
	{
		public MentalStateDef mentalState;

		public SimpleCurve mtbDaysMoodCurve;

		public DevelopmentalStage developmentalStageFilter = DevelopmentalStage.Child | DevelopmentalStage.Adult;

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (mentalState == null)
			{
				yield return "mentalState not set.";
			}
			if (mtbDaysMoodCurve == null)
			{
				yield return "mtbDaysMoodCurve not set.";
			}
		}

		public float CalculateMTBDays(Pawn pawn)
		{
			if (!developmentalStageFilter.Has(pawn.DevelopmentalStage))
			{
				return float.PositiveInfinity;
			}
			if (pawn.needs.mood == null)
			{
				return float.PositiveInfinity;
			}
			return mtbDaysMoodCurve?.Evaluate(pawn.needs.mood.CurLevelPercentage) ?? float.PositiveInfinity;
		}
	}
}
