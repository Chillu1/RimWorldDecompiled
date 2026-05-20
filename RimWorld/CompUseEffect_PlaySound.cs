using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompUseEffect_PlaySound : CompUseEffect
{
	private CompProperties_UseEffectPlaySound Props => (CompProperties_UseEffectPlaySound)props;

	public override void DoEffect(Pawn usedBy)
	{
		if (usedBy.Map == Find.CurrentMap && Props.soundOnUsed != null)
		{
			Props.soundOnUsed.PlayOneShot(SoundInfo.InMap(usedBy));
		}
	}
}
