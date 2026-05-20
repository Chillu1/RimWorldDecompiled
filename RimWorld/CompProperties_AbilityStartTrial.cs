namespace RimWorld
{
	public class CompProperties_AbilityStartTrial : CompProperties_AbilityStartRitualOnPawn
	{
		public PreceptDef ritualDefForPrisoner;

		public PreceptDef ritualDefForMentalState;

		public CompProperties_AbilityStartTrial()
		{
			compClass = typeof(CompAbilityEffect_StartTrial);
		}
	}
}
