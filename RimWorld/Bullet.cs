using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Bullet : Projectile
	{
		protected override void Impact(Thing hitThing)
		{
			Map map = base.Map;
			IntVec3 position = base.Position;
			base.Impact(hitThing);
			BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
			Find.BattleLog.Add(battleLogEntry_RangedImpact);
			NotifyImpact(hitThing, map, position);
			if (hitThing != null)
			{
				DamageInfo dinfo = new DamageInfo(def.projectile.damageDef, base.DamageAmount, base.ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
				hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);
				Pawn pawn = hitThing as Pawn;
				if (pawn != null && pawn.stances != null && pawn.BodySize <= def.projectile.StoppingPower + 0.001f)
				{
					pawn.stances.StaggerFor(95);
				}
				if (def.projectile.extraDamages == null)
				{
					return;
				}
				foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
				{
					if (Rand.Chance(extraDamage.chance))
					{
						DamageInfo dinfo2 = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing);
						hitThing.TakeDamage(dinfo2).AssociateWithLog(battleLogEntry_RangedImpact);
					}
				}
			}
			else
			{
				SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map));
				if (base.Position.GetTerrain(map).takeSplashes)
				{
					MoteMaker.MakeWaterSplash(ExactPosition, map, Mathf.Sqrt(base.DamageAmount) * 1f, 4f);
				}
				else
				{
					MoteMaker.MakeStaticMote(ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt);
				}
			}
		}

		private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
		{
			BulletImpactData bulletImpactData = default(BulletImpactData);
			bulletImpactData.bullet = this;
			bulletImpactData.hitThing = hitThing;
			bulletImpactData.impactPosition = position;
			BulletImpactData impactData = bulletImpactData;
			hitThing?.Notify_BulletImpactNearby(impactData);
			int num = 9;
			for (int i = 0; i < num; i++)
			{
				IntVec3 c = position + GenRadial.RadialPattern[i];
				if (!c.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j] != hitThing)
					{
						thingList[j].Notify_BulletImpactNearby(impactData);
					}
				}
			}
		}
	}
}
