using LudeonTK;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Verb_ArcSprayIncinerator : Verb_ShootBeam
{
	[TweakValue("Incinerator", 0f, 10f)]
	public static float DistanceToLifetimeScalar = 5f;

	[TweakValue("Incinerator", -2f, 7f)]
	public static float BarrelOffset = 5f;

	private IncineratorSpray sprayer;

	public override void WarmupComplete()
	{
		sprayer = GenSpawn.Spawn(ThingDefOf.IncineratorSpray, caster.Position, caster.Map) as IncineratorSpray;
		base.WarmupComplete();
		Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, currentTarget.HasThing ? currentTarget.Thing : null, base.EquipmentSource?.def, null, burst: false));
	}

	protected override bool TryCastShot()
	{
		bool result = base.TryCastShot();
		Vector3 vector = base.InterpolatedPosition.Yto0();
		IntVec3 intVec = vector.ToIntVec3();
		Vector3 drawPos = caster.DrawPos;
		Vector3 normalized = (vector - drawPos).normalized;
		drawPos += normalized * BarrelOffset;
		MoteDualAttached moteDualAttached = MoteMaker.MakeInteractionOverlay(A: new TargetInfo(caster.Position, caster.Map), moteDef: ThingDefOf.Mote_IncineratorBurst, B: new TargetInfo(intVec, caster.Map));
		float num = Vector3.Distance(vector, drawPos);
		float num2 = ((num < BarrelOffset) ? 0.5f : 1f);
		IncineratorSpray incineratorSpray = sprayer;
		if (incineratorSpray != null)
		{
			incineratorSpray.Add(new IncineratorProjectileMotion
			{
				mote = moteDualAttached,
				targetDest = intVec,
				worldSource = drawPos,
				worldTarget = vector,
				moveVector = (vector - drawPos).normalized,
				startScale = 1f * num2,
				endScale = (1f + Rand.Range(0.1f, 0.4f)) * num2,
				lifespanTicks = Mathf.FloorToInt(num * DistanceToLifetimeScalar)
			});
			return result;
		}
		return result;
	}
}
