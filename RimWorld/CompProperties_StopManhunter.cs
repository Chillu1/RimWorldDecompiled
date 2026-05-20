using Verse;

namespace RimWorld
{
	public class CompProperties_StopManhunter : CompProperties_AbilityEffect
	{
		[MustTranslate]
		public string successMessage;

		public CompProperties_StopManhunter()
		{
			compClass = typeof(CompAbilityEffect_StopManhunter);
		}
	}
}
