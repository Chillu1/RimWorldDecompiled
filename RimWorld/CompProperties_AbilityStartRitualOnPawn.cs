using Verse;

namespace RimWorld
{
	public class CompProperties_AbilityStartRitualOnPawn : CompProperties_AbilityStartRitual
	{
		[NoTranslate]
		public string targetRoleId;

		public CompProperties_AbilityStartRitualOnPawn()
		{
			compClass = typeof(CompAbilityEffect_StartRitualOnPawn);
		}
	}
}
