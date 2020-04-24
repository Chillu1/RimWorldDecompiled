using Verse;

namespace RimWorld
{
	public class HediffCompProperties_PsychicHarmonizer : HediffCompProperties
	{
		public float range;

		public ThoughtDef thought;

		public HediffCompProperties_PsychicHarmonizer()
		{
			compClass = typeof(HediffComp_PsychicHarmonizer);
		}
	}
}
