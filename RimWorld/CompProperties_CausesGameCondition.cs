using Verse;

namespace RimWorld
{
	public class CompProperties_CausesGameCondition : CompProperties
	{
		public GameConditionDef conditionDef;

		public int worldRange;

		public bool preventConditionStacking = true;

		public CompProperties_CausesGameCondition()
		{
			compClass = typeof(CompCauseGameCondition);
		}
	}
}
