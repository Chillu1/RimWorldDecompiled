using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class GasUtility
{
	public const float BlindingGasAccuracyPenalty = 0.7f;

	public const int RotStinkPerRawMeatRotting = 2;

	private const float RotStinkPerBodySize = 52f;

	private const float RotStinkHumanlikeFactor = 1.15f;

	private const int GasCheckInterval = 50;

	private const float ToxGasEffectOnExtremeBuildupFactor = 0.25f;

	private const int DeadlifeShamblerLifespanTicks = 15000;

	private static List<Thing> tempThingList = new List<Thing>();

	public static string GetLabel(this GasType gasType)
	{
		switch (gasType)
		{
		case GasType.BlindSmoke:
			return "BlindSmoke".Translate();
		case GasType.ToxGas:
			return "ToxGas".Translate();
		case GasType.RotStink:
			return "RotStink".Translate();
		case GasType.DeadlifeDust:
			return "DeadlifeDust".Translate();
		default:
			Log.ErrorOnce("Trying to get unknown gas type label.", 172091);
			return string.Empty;
		}
	}

	public static void AddGas(IntVec3 cell, Map map, GasType gasType, float radius)
	{
		int num = GenRadial.NumCellsInRadius(radius);
		map.gasGrid.AddGas(cell, gasType, 255 * num);
	}

	public static void AddGas(IntVec3 cell, Map map, GasType gasType, int amount)
	{
		map.gasGrid.AddGas(cell, gasType, amount);
	}

	public static void AddDeadifeGas(IntVec3 cell, Map map, Faction faction, int amount)
	{
		if (ModsConfig.AnomalyActive)
		{
			MarkDeadlifeCorpsesForFaction(cell, map, faction, amount);
			map.gasGrid.AddGas(cell, GasType.DeadlifeDust, amount);
		}
	}

	public static void MarkDeadlifeCorpsesForFaction(IntVec3 cell, Map map, Faction faction, int amount)
	{
		map.gasGrid.EstimateGasDiffusion(cell, GasType.DeadlifeDust, amount, delegate(IntVec3 c)
		{
			foreach (Thing thing in c.GetThingList(map))
			{
				if (thing is Corpse corpse)
				{
					corpse.InnerPawn.MarkDeadlifeDustForFaction(faction);
				}
			}
		});
	}

	public static byte GasDensity(this IntVec3 cell, Map map, GasType gasType)
	{
		return map.gasGrid.DensityAt(cell, gasType);
	}

	public static bool AnyGas(this IntVec3 cell, Map map, GasType gasType)
	{
		return map.gasGrid.DensityAt(cell, gasType) > 0;
	}

	public static int RotStinkToSpawnForCorpse(Corpse corpse)
	{
		if (GenTemperature.RotRateAtTemperature(Mathf.RoundToInt(corpse.AmbientTemperature)) <= 0f)
		{
			return 0;
		}
		if (corpse.GetRotStage() == RotStage.Rotting)
		{
			float num = corpse.InnerPawn.BodySize;
			if (corpse.InnerPawn.RaceProps.Humanlike)
			{
				num *= 1.15f;
			}
			return Mathf.CeilToInt(num * 52f);
		}
		return 0;
	}

	public static void PawnGasEffectsTickInterval(Pawn pawn, int delta)
	{
		if (!pawn.Spawned || !pawn.IsHashIntervalTick(50, delta))
		{
			return;
		}
		if (pawn.Position.GasDensity(pawn.Map, GasType.RotStink) > 0 && (pawn.RaceProps.Animal || pawn.RaceProps.Humanlike) && (!pawn.IsMutant || !pawn.mutant.Def.isImmuneToInfections) && !pawn.RaceProps.isImmuneToInfections && !pawn.health.hediffSet.HasHediff(HediffDefOf.LungRotExposure) && GetLungRotAffectedBodyParts(pawn).Any())
		{
			pawn.health.AddHediff(HediffDefOf.LungRotExposure);
		}
		if (ModsConfig.BiotechActive && pawn.Spawned)
		{
			byte b = pawn.Position.GasDensity(pawn.Map, GasType.ToxGas);
			if (b > 0)
			{
				float num = (float)(int)b / 255f;
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
				if (firstHediffOfDef != null && firstHediffOfDef.CurStageIndex == firstHediffOfDef.def.stages.Count - 1)
				{
					num *= 0.25f;
				}
				if (ShouldGetGasExposureHediff(pawn))
				{
					pawn.health.AddHediff(HediffDefOf.ToxGasExposure);
				}
				ToxicUtility.DoPawnToxicDamage(pawn, num);
			}
		}
		if (ModsConfig.AnomalyActive && pawn.Spawned && pawn.Position.GasDensity(pawn.Map, GasType.DeadlifeDust) > 0 && pawn.IsShambler)
		{
			HediffComp_DisappearsAndKills hediffComp_DisappearsAndKills = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Shambler)?.TryGetComp<HediffComp_DisappearsAndKills>();
			if (hediffComp_DisappearsAndKills != null)
			{
				hediffComp_DisappearsAndKills.ticksToDisappear = Mathf.Max(hediffComp_DisappearsAndKills.ticksToDisappear, 15000);
			}
		}
	}

	public static void CorpseGasEffectsTickRare(Corpse corpse)
	{
		if (!corpse.Spawned)
		{
			return;
		}
		if (corpse.GetRotStage() == RotStage.Rotting)
		{
			int num = RotStinkToSpawnForCorpse(corpse);
			if (num > 0)
			{
				AddGas(corpse.Position, corpse.Map, GasType.RotStink, num);
			}
		}
		if (ModsConfig.AnomalyActive && corpse.Position.GasDensity(corpse.Map, GasType.DeadlifeDust) > 0 && !corpse.InnerPawn.health.hediffSet.HasHediff(HediffDefOf.Shambler) && MutantUtility.CanResurrectAsShambler(corpse, ignoreIndoors: true))
		{
			MutantUtility.ResurrectAsShambler(corpse.InnerPawn, 15000, corpse.InnerPawn.DeadlifeDustFaction);
		}
	}

	public static void DoSteadyEffects(IntVec3 cell, Map map)
	{
		if (!ModsConfig.AnomalyActive || cell.GasDensity(map, GasType.DeadlifeDust) <= 0)
		{
			return;
		}
		tempThingList.Clear();
		tempThingList.AddRange(cell.GetThingList(map));
		foreach (Thing tempThing in tempThingList)
		{
			if (tempThing is Building_Grave { Corpse: not null } building_Grave && MutantUtility.CanResurrectAsShambler(building_Grave.Corpse, ignoreIndoors: true))
			{
				Corpse corpse = building_Grave.Corpse;
				building_Grave.EjectContents();
				MutantUtility.ResurrectAsShambler(corpse.InnerPawn, 15000, corpse.InnerPawn.DeadlifeDustFaction);
			}
		}
	}

	public static IEnumerable<BodyPartRecord> GetLungRotAffectedBodyParts(Pawn pawn)
	{
		return from p in pawn.health.hediffSet.GetNotMissingParts()
			where p.def == BodyPartDefOf.Lung && !pawn.health.hediffSet.hediffs.Any((Hediff x) => x.Part == p && x.def.preventsLungRot)
			select p;
	}

	private static bool ShouldGetGasExposureHediff(Pawn pawn)
	{
		if (IsAffectedByExposure(pawn))
		{
			return !pawn.health.hediffSet.HasHediff(HediffDefOf.ToxGasExposure);
		}
		return false;
	}

	public static bool IsAffectedByExposure(Pawn pawn)
	{
		if (pawn.RaceProps.Humanlike || pawn.RaceProps.Humanlike)
		{
			if (pawn.apparel != null)
			{
				foreach (Apparel item in pawn.apparel.WornApparel)
				{
					if (item.def.apparel.immuneToToxGasExposure)
					{
						return false;
					}
				}
			}
			if (pawn.genes != null)
			{
				foreach (Gene item2 in pawn.genes.GenesListForReading)
				{
					if (item2.def.immuneToToxGasExposure)
					{
						return false;
					}
				}
			}
			return true;
		}
		return false;
	}
}
