using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompPlaySoundOnDestroy : ThingComp
{
	private CompProperties_PlaySoundOnDestroy Props => (CompProperties_PlaySoundOnDestroy)props;

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		if ((mode != DestroyMode.Vanish || !Props.ignoreOnVanish) && (!Props.onlyWhenKilled || mode == DestroyMode.KillFinalize || mode == DestroyMode.KillFinalizeLeavingsOnly) && previousMap != null)
		{
			Props.sound.PlayOneShotOnCamera(previousMap);
		}
	}
}
