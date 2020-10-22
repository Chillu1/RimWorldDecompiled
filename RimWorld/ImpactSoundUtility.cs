using Verse;
using Verse.Sound;

namespace RimWorld
{
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
				soundDef = ((hitThing.Stuff == null) ? hitThing.def.soundImpactDefault : hitThing.Stuff.stuffProps.soundImpactStuff);
				if (soundDef.NullOrUndefined())
				{
					soundDef = SoundDefOf.BulletImpact_Ground;
				}
			}
			if (!soundDef.NullOrUndefined())
			{
				soundDef.PlayOneShot(new TargetInfo(hitThing.PositionHeld, map));
			}
		}
	}
}
