namespace RimWorld
{
	public class CompProperties_RitualEffectDrum : CompProperties_RitualEffectIntervalSpawn
	{
		public int maxDistance;

		public float maxOffset;

		public CompProperties_RitualEffectDrum()
		{
			compClass = typeof(CompRitualEffect_Drum);
		}
	}
}
