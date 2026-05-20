using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Verb_LaunchProjectile : Verb
{
	private List<IntVec3> forcedMissTargetEvenDispersalCache = new List<IntVec3>();

	public override float EffectiveRange => base.EffectiveRange * (base.EquipmentSource?.GetStatValue(StatDefOf.RangedWeapon_RangeMultiplier) ?? 1f);

	public override float WarmupTime => base.WarmupTime * (base.EquipmentSource?.GetStatValue(StatDefOf.RangedWeapon_WarmupMultiplier) ?? 1f);

	public virtual ThingDef Projectile
	{
		get
		{
			CompChangeableProjectile compChangeableProjectile = base.EquipmentSource?.GetComp<CompChangeableProjectile>();
			if (compChangeableProjectile != null && compChangeableProjectile.Loaded)
			{
				return compChangeableProjectile.Projectile;
			}
			return verbProps.defaultProjectile;
		}
	}

	public override void WarmupComplete()
	{
		base.WarmupComplete();
		Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, currentTarget.HasThing ? currentTarget.Thing : null, base.EquipmentSource?.def, Projectile, ShotsPerBurst > 1));
	}

	protected IntVec3 GetForcedMissTarget(float forcedMissRadius)
	{
		if (verbProps.forcedMissEvenDispersal)
		{
			if (forcedMissTargetEvenDispersalCache.Count <= 0)
			{
				forcedMissTargetEvenDispersalCache.AddRange(GenerateEvenDispersalForcedMissTargets(currentTarget.Cell, forcedMissRadius, burstShotsLeft));
				forcedMissTargetEvenDispersalCache.SortByDescending((IntVec3 p) => p.DistanceToSquared(Caster.Position));
			}
			if (forcedMissTargetEvenDispersalCache.Count > 0)
			{
				return forcedMissTargetEvenDispersalCache.Pop();
			}
		}
		int maxExclusive = GenRadial.NumCellsInRadius(forcedMissRadius);
		int num = Rand.Range(0, maxExclusive);
		return currentTarget.Cell + GenRadial.RadialPattern[num];
	}

	private static IEnumerable<IntVec3> GenerateEvenDispersalForcedMissTargets(IntVec3 root, float radius, int count)
	{
		float randomRotationOffset = Rand.Range(0f, 360f);
		float goldenRatio = (1f + Mathf.Pow(5f, 0.5f)) / 2f;
		for (int i = 0; i < count; i++)
		{
			float f = MathF.PI * 2f * (float)i / goldenRatio;
			float f2 = Mathf.Acos(1f - 2f * ((float)i + 0.5f) / (float)count);
			int num = (int)(Mathf.Cos(f) * Mathf.Sin(f2) * radius);
			int num2 = (int)(Mathf.Cos(f2) * radius);
			Vector3 vect = new Vector3(num, 0f, num2).RotatedBy(randomRotationOffset);
			yield return root + vect.ToIntVec3();
		}
	}

	protected override bool TryCastShot()
	{
		if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
		{
			return false;
		}
		ThingDef projectile = Projectile;
		if (projectile == null)
		{
			return false;
		}
		ShootLine resultingLine;
		bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
		if (verbProps.stopBurstWithoutLos && !flag)
		{
			return false;
		}
		if (base.EquipmentSource != null)
		{
			base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
			base.EquipmentSource.GetComp<CompApparelVerbOwner_Charged>()?.UsedOnce();
		}
		lastShotTick = Find.TickManager.TicksGame;
		Thing manningPawn = caster;
		Thing equipmentSource = base.EquipmentSource;
		CompMannable compMannable = caster.TryGetComp<CompMannable>();
		if (compMannable?.ManningPawn != null)
		{
			manningPawn = compMannable.ManningPawn;
			equipmentSource = caster;
		}
		Vector3 drawPos = caster.DrawPos;
		Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
		if (equipmentSource.TryGetComp(out CompUniqueWeapon comp))
		{
			foreach (WeaponTraitDef item in comp.TraitsListForReading)
			{
				if (item.damageDefOverride != null)
				{
					projectile2.damageDefOverride = item.damageDefOverride;
				}
				if (!item.extraDamages.NullOrEmpty())
				{
					Projectile projectile3 = projectile2;
					if (projectile3.extraDamages == null)
					{
						projectile3.extraDamages = new List<ExtraDamage>();
					}
					projectile2.extraDamages.AddRange(item.extraDamages);
				}
			}
		}
		if (verbProps.ForcedMissRadius > 0.5f)
		{
			float num = verbProps.ForcedMissRadius;
			if (manningPawn is Pawn pawn)
			{
				num *= verbProps.GetForceMissFactorFor(equipmentSource, pawn);
			}
			float num2 = VerbUtility.CalculateAdjustedForcedMiss(num, currentTarget.Cell - caster.Position);
			if (num2 > 0.5f)
			{
				IntVec3 forcedMissTarget = GetForcedMissTarget(num2);
				if (forcedMissTarget != currentTarget.Cell)
				{
					ThrowDebugText("ToRadius");
					ThrowDebugText("Rad\nDest", forcedMissTarget);
					ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
					if (Rand.Chance(0.5f))
					{
						projectileHitFlags = ProjectileHitFlags.All;
					}
					if (!canHitNonTargetPawnsNow)
					{
						projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
					}
					projectile2.Launch(manningPawn, drawPos, forcedMissTarget, currentTarget, projectileHitFlags, preventFriendlyFire, equipmentSource);
					return true;
				}
			}
		}
		ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
		Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
		ThingDef targetCoverDef = randomCoverToMissInto?.def;
		if (verbProps.canGoWild && !Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
		{
			bool flyOverhead = projectile2?.def?.projectile != null && projectile2.def.projectile.flyOverhead;
			resultingLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget, flyOverhead, caster.Map);
			ThrowDebugText("ToWild" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
			ThrowDebugText("Wild\nDest", resultingLine.Dest);
			ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
			if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
			{
				projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
			}
			projectile2.Launch(manningPawn, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags2, preventFriendlyFire, equipmentSource, targetCoverDef);
			return true;
		}
		if (currentTarget.Thing != null && currentTarget.Thing.def.CanBenefitFromCover && !Rand.Chance(shotReport.PassCoverChance))
		{
			ThrowDebugText("ToCover" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
			ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
			ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
			if (canHitNonTargetPawnsNow)
			{
				projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
			}
			projectile2.Launch(manningPawn, drawPos, randomCoverToMissInto, currentTarget, projectileHitFlags3, preventFriendlyFire, equipmentSource, targetCoverDef);
			return true;
		}
		ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
		if (canHitNonTargetPawnsNow)
		{
			projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
		}
		if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
		{
			projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
		}
		ThrowDebugText("ToHit" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
		if (currentTarget.Thing != null)
		{
			projectile2.Launch(manningPawn, drawPos, currentTarget, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, targetCoverDef);
			ThrowDebugText("Hit\nDest", currentTarget.Cell);
		}
		else
		{
			projectile2.Launch(manningPawn, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, preventFriendlyFire, equipmentSource, targetCoverDef);
			ThrowDebugText("Hit\nDest", resultingLine.Dest);
		}
		return true;
	}

	private void ThrowDebugText(string text)
	{
		if (DebugViewSettings.drawShooting)
		{
			MoteMaker.ThrowText(caster.DrawPos, caster.Map, text);
		}
	}

	private void ThrowDebugText(string text, IntVec3 c)
	{
		if (DebugViewSettings.drawShooting)
		{
			MoteMaker.ThrowText(c.ToVector3Shifted(), caster.Map, text);
		}
	}

	public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
	{
		needLOSToCenter = true;
		ThingDef projectile = Projectile;
		if (projectile == null)
		{
			return 0f;
		}
		float num = projectile.projectile.explosionRadius + projectile.projectile.explosionRadiusDisplayPadding;
		float forcedMissRadius = verbProps.ForcedMissRadius;
		if (forcedMissRadius > 0f && base.BurstShotCount > 1)
		{
			num += forcedMissRadius;
		}
		return num;
	}

	public override bool Available()
	{
		if (!base.Available())
		{
			return false;
		}
		if (CasterIsPawn)
		{
			Pawn casterPawn = CasterPawn;
			if (casterPawn.Faction != Faction.OfPlayer && !verbProps.ai_ProjectileLaunchingIgnoresMeleeThreats && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
			{
				return false;
			}
		}
		return Projectile != null;
	}

	public override void Reset()
	{
		base.Reset();
		forcedMissTargetEvenDispersalCache.Clear();
	}
}
