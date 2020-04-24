using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Mineable : Building
	{
		private float yieldPct;

		private const float YieldChanceOnNonMiningKill = 0.2f;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref yieldPct, "yieldPct", 0f);
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(ref dinfo, out absorbed);
			if (!absorbed)
			{
				if (def.building.mineableThing != null && def.building.mineableYieldWasteable && dinfo.Def == DamageDefOf.Mining && dinfo.Instigator != null && dinfo.Instigator is Pawn)
				{
					Notify_TookMiningDamage(GenMath.RoundRandom(dinfo.Amount), (Pawn)dinfo.Instigator);
				}
				absorbed = false;
			}
		}

		public void DestroyMined(Pawn pawn)
		{
			Map map = base.Map;
			base.Destroy(DestroyMode.KillFinalize);
			TrySpawnYield(map, yieldPct, moteOnWaste: true, pawn);
		}

		public override void Destroy(DestroyMode mode)
		{
			Map map = base.Map;
			base.Destroy(mode);
			if (mode == DestroyMode.KillFinalize)
			{
				TrySpawnYield(map, 0.2f, moteOnWaste: false, null);
			}
		}

		private void TrySpawnYield(Map map, float yieldChance, bool moteOnWaste, Pawn pawn)
		{
			if (def.building.mineableThing != null && !(Rand.Value > def.building.mineableDropChance))
			{
				int num = Mathf.Max(1, def.building.mineableYield);
				if (def.building.mineableYieldWasteable)
				{
					num = Mathf.Max(1, GenMath.RoundRandom((float)num * yieldPct));
				}
				Thing thing = ThingMaker.MakeThing(def.building.mineableThing);
				thing.stackCount = num;
				GenSpawn.Spawn(thing, base.Position, map);
				if ((pawn == null || !pawn.IsColonist) && thing.def.EverHaulable && !thing.def.designateHaulable)
				{
					thing.SetForbidden(value: true);
				}
			}
		}

		public void Notify_TookMiningDamage(int amount, Pawn miner)
		{
			float num = (float)Mathf.Min(amount, HitPoints) / (float)base.MaxHitPoints;
			yieldPct += num * miner.GetStatValue(StatDefOf.MiningYield);
		}
	}
}
