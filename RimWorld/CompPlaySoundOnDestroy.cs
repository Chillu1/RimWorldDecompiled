using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompPlaySoundOnDestroy : ThingComp
	{
		private CompProperties_PlaySoundOnDestroy Props => (CompProperties_PlaySoundOnDestroy)props;

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			if (previousMap != null)
			{
				Props.sound.PlayOneShotOnCamera(previousMap);
			}
		}
	}
}
