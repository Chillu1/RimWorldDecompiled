using Verse;

namespace RimWorld
{
	public class CompProperties_UseEffectPlaySound : CompProperties_Usable
	{
		public SoundDef soundOnUsed;

		public CompProperties_UseEffectPlaySound()
		{
			compClass = typeof(CompUseEffect_PlaySound);
		}
	}
}
