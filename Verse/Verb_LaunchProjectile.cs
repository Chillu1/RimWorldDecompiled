using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Verb_LaunchProjectile : Verb
	{
		public virtual ThingDef Projectile
		{
			get
			{
				if (base.EquipmentSource != null)
				{
					CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
					if (comp != null && comp.Loaded)
					{
						return comp.Projectile;
					}
				}
				return verbProps.defaultProjectile;
			}
		}

		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, currentTarget.HasThing ? currentTarget.Thing : null, (base.EquipmentSource != null) ? base.EquipmentSource.def : null, Projectile, ShotsPerBurst > 1));
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
				base.EquipmentSource.GetComp<CompReloadable>()?.UsedOnce();
			}
			Thing launcher = caster;
			Thing equipment = base.EquipmentSource;
			CompMannable compMannable = caster.TryGetComp<CompMannable>();
			if (compMannable != null && compMannable.ManningPawn != null)
			{
				launcher = compMannable.ManningPawn;
				equipment = caster;
			}
			Vector3 drawPos = caster.DrawPos;
			Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
			if (verbProps.forcedMissRadius > 0.5f)
			{
				float num = VerbUtility.CalculateAdjustedForcedMiss(verbProps.forcedMissRadius, currentTarget.Cell - caster.Position);
				if (num > 0.5f)
				{
					int max = GenRadial.NumCellsInRadius(num);
					int num2 = Rand.Range(0, max);
					if (num2 > 0)
					{
						IntVec3 c = currentTarget.Cell + GenRadial.RadialPattern[num2];
						ThrowDebugText("ToRadius");
						ThrowDebugText("Rad\nDest", c);
						ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
						if (Rand.Chance(0.5f))
						{
							projectileHitFlags = ProjectileHitFlags.All;
						}
						if (!canHitNonTargetPawnsNow)
						{
							projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
						}
						projectile2.Launch(launcher, drawPos, c, currentTarget, projectileHitFlags, equipment);
						return true;
					}
				}
			}
			ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
			Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
			ThingDef targetCoverDef = randomCoverToMissInto?.def;
			if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
			{
				resultingLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
				ThrowDebugText("ToWild" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
				ThrowDebugText("Wild\nDest", resultingLine.Dest);
				ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
				if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
				{
					projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
				}
				projectile2.Launch(launcher, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags2, equipment, targetCoverDef);
				return true;
			}
			if (currentTarget.Thing != null && currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
			{
				ThrowDebugText("ToCover" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
				ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
				ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
				if (canHitNonTargetPawnsNow)
				{
					projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
				}
				projectile2.Launch(launcher, drawPos, randomCoverToMissInto, currentTarget, projectileHitFlags3, equipment, targetCoverDef);
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
				projectile2.Launch(launcher, drawPos, currentTarget, currentTarget, projectileHitFlags4, equipment, targetCoverDef);
				ThrowDebugText("Hit\nDest", currentTarget.Cell);
			}
			else
			{
				projectile2.Launch(launcher, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, equipment, targetCoverDef);
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
			return Projectile?.projectile.explosionRadius ?? 0f;
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
				if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
				{
					return false;
				}
			}
			return Projectile != null;
		}
	}
}
