using Verse;

namespace RimWorld
{
	public class CompProperties_UseEffect : CompProperties
	{
		public bool doCameraShake;

		public ThingDef moteOnUsed;

		public float moteOnUsedScale = 1f;

		public CompProperties_UseEffect()
		{
			compClass = typeof(CompUseEffect);
		}
	}
}
