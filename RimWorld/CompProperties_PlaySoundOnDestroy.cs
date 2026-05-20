using Verse;

namespace RimWorld;

public class CompProperties_PlaySoundOnDestroy : CompProperties
{
	public SoundDef sound;

	public bool onlyWhenKilled;

	public bool ignoreOnVanish = true;

	public CompProperties_PlaySoundOnDestroy()
	{
		compClass = typeof(CompPlaySoundOnDestroy);
	}
}
