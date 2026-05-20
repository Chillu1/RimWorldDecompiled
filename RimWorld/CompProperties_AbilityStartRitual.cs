namespace RimWorld
{
	public class CompProperties_AbilityStartRitual : CompProperties_AbilityEffect
	{
		public PreceptDef ritualDef;

		public bool allowedForChild = true;

		public CompProperties_AbilityStartRitual()
		{
			compClass = typeof(CompAbilityEffect_StartRitual);
		}
	}
}
