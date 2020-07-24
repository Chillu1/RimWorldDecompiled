using Verse;

namespace RimWorld
{
	public class CompProperties_PlaySoundOnDestroy : CompProperties
	{
		public SoundDef sound;

		public CompProperties_PlaySoundOnDestroy()
		{
			compClass = typeof(CompPlaySoundOnDestroy);
		}
	}
}
