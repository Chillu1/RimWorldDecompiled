namespace Verse
{
	public class CompProperties_AffectsSky : CompProperties
	{
		public float glow = 1f;

		public SkyColorSet skyColors;

		public float lightsourceShineSize = 1f;

		public float lightsourceShineIntensity = 1f;

		public bool lerpDarken;

		public CompProperties_AffectsSky()
		{
			compClass = typeof(CompAffectsSky);
		}
	}
}
