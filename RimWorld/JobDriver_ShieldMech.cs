using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ShieldMech : JobDriver_CastAbility
{
	private const int DurationTicks = 1800;

	private MechShield mechShield;

	private CompProjectileInterceptor projectileInterceptor;

	private CompProjectileInterceptor ProjectileInterceptor
	{
		get
		{
			if (projectileInterceptor == null && mechShield != null)
			{
				projectileInterceptor = mechShield.GetComp<CompProjectileInterceptor>();
			}
			return projectileInterceptor;
		}
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		foreach (Toil item in base.MakeNewToils())
		{
			yield return item;
		}
		AddFinishAction(delegate
		{
			if (mechShield != null)
			{
				if (!mechShield.Destroyed)
				{
					mechShield.Destroy();
				}
				mechShield = null;
			}
		});
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			pawn.pather.StopDead();
			mechShield = (MechShield)GenSpawn.Spawn(ThingDefOf.MechShield, base.TargetThingA.Position, pawn.Map);
			mechShield.SetTarget(base.TargetThingA);
			int num = (int)pawn.GetStatValue(StatDefOf.MechRemoteShieldEnergy);
			ProjectileInterceptor.maxHitPointsOverride = num;
			ProjectileInterceptor.currentHitPoints = num;
		};
		toil.tickIntervalAction = delegate
		{
			pawn.rotationTracker.FaceTarget(base.TargetThingA);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 1800;
		toil.handlingFacing = true;
		toil.PlaySustainerOrSound(SoundDefOf.ShieldMech);
		toil.AddFailCondition(delegate
		{
			if (mechShield != null)
			{
				if (!job.ability.verb.CanHitTargetFrom(pawn.Position, mechShield))
				{
					return true;
				}
				if (ProjectileInterceptor != null && !ProjectileInterceptor.Active)
				{
					return true;
				}
			}
			return false;
		});
		yield return toil;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref mechShield, "mechShield");
	}
}
