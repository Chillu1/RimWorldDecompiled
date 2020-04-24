using Verse;

namespace RimWorld
{
	public static class RottableUtility
	{
		public static bool IsNotFresh(this Thing t)
		{
			CompRottable compRottable = t.TryGetComp<CompRottable>();
			if (compRottable != null)
			{
				return compRottable.Stage != RotStage.Fresh;
			}
			return false;
		}

		public static bool IsDessicated(this Thing t)
		{
			CompRottable compRottable = t.TryGetComp<CompRottable>();
			if (compRottable != null)
			{
				return compRottable.Stage == RotStage.Dessicated;
			}
			return false;
		}

		public static RotStage GetRotStage(this Thing t)
		{
			return t.TryGetComp<CompRottable>()?.Stage ?? RotStage.Fresh;
		}
	}
}
