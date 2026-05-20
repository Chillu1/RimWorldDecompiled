using Verse;
using Verse.Sound;

namespace RimWorld;

public static class ImpactSoundUtility
{
	public static void PlayImpactSound(Thing hitThing, ImpactSoundTypeDef ist, Map map)
	{
		if (ist == null || (ist.playOnlyIfHitPawn && !(hitThing is Pawn)))
		{
			return;
		}
		if (map == null)
		{
			Log.Warning("Can't play impact sound because map is null.");
			return;
		}
		SoundDef soundDef;
		if (hitThing is Pawn)
		{
			soundDef = ist.soundDef;
		}
		else
		{
			soundDef = ((hitThing.Stuff == null) ? hitThing.def.soundImpactDefault : ((ist != ImpactSoundTypeDefOf.Bullet) ? hitThing.Stuff.stuffProps.soundImpactMelee : hitThing.Stuff.stuffProps.soundImpactBullet));
			if (soundDef.NullOrUndefined())
			{
				soundDef = SoundDefOf.Pawn_Melee_Punch_HitBuilding_Generic;
			}
		}
		if (!soundDef.NullOrUndefined())
		{
			soundDef.PlayOneShot(new TargetInfo(hitThing.PositionHeld, map));
		}
	}
}
