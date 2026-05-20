using Verse;

namespace RimWorld
{
	public class CompProperties_BandNode : CompProperties
	{
		public HediffDef hediff;

		public float retuneDays = 3f;

		public float tuneSeconds = 5f;

		public int powerConsumptionIdle = 100;

		public int emissionInterval;

		public EffecterDef untunedEffect;

		public EffecterDef tuningEffect;

		public EffecterDef tunedEffect;

		public EffecterDef retuningEffect;

		public SoundDef tuningCompleteSound;

		public CompProperties_BandNode()
		{
			compClass = typeof(CompBandNode);
		}
	}
}
