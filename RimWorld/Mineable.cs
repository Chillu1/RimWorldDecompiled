using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Mineable : Building
	{
		private float yieldPct;

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

		public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
		{
			if (dinfo.HasValue && dinfo.Value.Instigator != null && dinfo.Value.Def != DamageDefOf.Mining)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.DestroyedMineable, dinfo.Value.Instigator.Named(HistoryEventArgsNames.Doer)));
			}
			base.Kill(dinfo, exactCulprit);
		}

		public void DestroyMined(Pawn pawn)
		{
			Map map = base.Map;
			base.Destroy(DestroyMode.KillFinalize);
			TrySpawnYield(map, moteOnWaste: true, pawn);
			if (pawn != null)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.Mined, pawn.Named(HistoryEventArgsNames.Doer)));
			}
		}

		public override void Destroy(DestroyMode mode)
		{
			Map map = base.Map;
			base.Destroy(mode);
			if (mode == DestroyMode.KillFinalize)
			{
				TrySpawnYield(map, moteOnWaste: false, null);
			}
		}

		[Obsolete("Will be removeds.")]
		private void TrySpawnYield(Map map, float yieldChance, bool moteOnWaste, Pawn pawn)
		{
			TrySpawnYield(map, moteOnWaste, pawn);
		}

		private void TrySpawnYield(Map map, bool moteOnWaste, Pawn pawn)
		{
			if (def.building.mineableThing != null && !(Rand.Value > def.building.mineableDropChance))
			{
				int num = Mathf.Max(1, def.building.EffectiveMineableYield);
				if (def.building.mineableYieldWasteable)
				{
					num = Mathf.Max(1, GenMath.RoundRandom((float)num * yieldPct));
				}
				Thing thing = ThingMaker.MakeThing(def.building.mineableThing);
				thing.stackCount = num;
				GenPlace.TryPlaceThing(thing, base.Position, map, ThingPlaceMode.Near, ForbidIfNecessary);
			}
			void ForbidIfNecessary(Thing thing2, int count)
			{
				if ((pawn == null || pawn.Faction != Faction.OfPlayer) && thing2.def.EverHaulable && !thing2.def.designateHaulable)
				{
					thing2.SetForbidden(value: true, warnOnFail: false);
				}
			}
		}

		public void Notify_TookMiningDamage(int amount, Pawn miner)
		{
			float num = (float)Mathf.Min(amount, HitPoints) / (float)base.MaxHitPoints;
			float num2 = 1f;
			if (miner != null)
			{
				num2 = miner.GetStatValue(StatDefOf.MiningYield);
			}
			yieldPct += num * num2;
		}
	}
}
