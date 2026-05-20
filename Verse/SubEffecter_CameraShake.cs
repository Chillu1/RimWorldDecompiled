using UnityEngine;
using Verse.Sound;

namespace Verse;

public abstract class SubEffecter_CameraShake : SubEffecter
{
	public SubEffecter_CameraShake(SubEffecterDef subDef, Effecter parent)
		: base(subDef, parent)
	{
	}

	public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
	{
		if (force)
		{
			DoShake(A);
		}
	}

	public void DoShake(TargetInfo tgt)
	{
		float num = 1f - Find.CameraDriver.ZoomRootSize.Remap(11f, 60f, 0f, 1f);
		IntVec3 mapPosition = Find.CameraDriver.MapPosition;
		float lengthHorizontal = (tgt.Cell - mapPosition).LengthHorizontal;
		float num2 = 1f - Mathf.Clamp01(lengthHorizontal / def.distanceAttenuationMax);
		num2 *= num;
		float randomInRange = def.cameraShake.RandomInRange;
		Find.CameraDriver.shaker.DoShake(randomInRange * Mathf.Lerp(1f, num2, def.distanceAttenuationScale));
		if (def.soundDef != null)
		{
			def.soundDef.PlayOneShot(tgt);
		}
	}
}
