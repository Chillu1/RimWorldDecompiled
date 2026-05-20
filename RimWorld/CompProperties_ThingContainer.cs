using Verse;

namespace RimWorld
{
	public class CompProperties_ThingContainer : CompProperties
	{
		public int stackLimit;

		public int minCountToEmpty;

		public IntVec3 containedThingOffset;

		public bool drawStackLabel;

		public bool drawContainedThing = true;

		public EffecterDef dropEffecterDef;
	}
}
