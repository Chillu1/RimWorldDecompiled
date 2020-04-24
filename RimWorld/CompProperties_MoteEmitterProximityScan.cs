namespace RimWorld
{
	public class CompProperties_MoteEmitterProximityScan : CompProperties_MoteEmitter
	{
		public float warmupPulseFadeInTime;

		public float warmupPulseSolidTime;

		public float warmupPulseFadeOutTime;

		public CompProperties_MoteEmitterProximityScan()
		{
			compClass = typeof(CompMoteEmitterProximityScan);
		}
	}
}
