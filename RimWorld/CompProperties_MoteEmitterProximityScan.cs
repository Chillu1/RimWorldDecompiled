using Verse;

namespace RimWorld;

public class CompProperties_MoteEmitterProximityScan : CompProperties_MoteEmitter
{
	public float warmupPulseFadeInTime;

	public float warmupPulseSolidTime;

	public float warmupPulseFadeOutTime;

	public SoundDef soundEmitting;

	public CompProperties_MoteEmitterProximityScan()
	{
		compClass = typeof(CompMoteEmitterProximityScan);
	}
}
