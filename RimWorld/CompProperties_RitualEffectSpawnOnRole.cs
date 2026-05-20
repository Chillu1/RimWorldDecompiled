using Verse;

namespace RimWorld
{
	public class CompProperties_RitualEffectSpawnOnRole : CompProperties_RitualEffectSpawnOnPawn
	{
		[NoTranslate]
		public string roleId;

		public CompProperties_RitualEffectSpawnOnRole()
		{
			compClass = typeof(CompRitualEffect_SpawnOnRole);
		}
	}
}
