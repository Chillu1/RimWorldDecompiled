using Verse;

namespace RimWorld;

public class CompProperties_AmbientSound : CompProperties
{
	public SoundDef sound;

	public bool disabledOnUnpowered;

	public bool disableOnHacked;

	public bool disableOnInteracted;

	public CompProperties_AmbientSound()
	{
		compClass = typeof(CompAmbientSound);
	}
}
