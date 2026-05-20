using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_GrowerSow : WorkGiver_Grower
{
	protected static string CantSowCavePlantBecauseOfLightTrans;

	protected static string CantSowCavePlantBecauseUnroofedTrans;

	public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

	public static void ResetStaticData()
	{
		CantSowCavePlantBecauseOfLightTrans = "CantSowCavePlantBecauseOfLight".Translate();
		CantSowCavePlantBecauseUnroofedTrans = "CantSowCavePlantBecauseUnroofed".Translate();
	}

	protected override bool ExtraRequirements(IPlantToGrowSettable settable, Pawn pawn)
	{
		if (!settable.CanAcceptSowNow())
		{
			return false;
		}
		IntVec3 c;
		if (settable is Zone_Growing zone_Growing)
		{
			if (!zone_Growing.allowSow)
			{
				return false;
			}
			c = zone_Growing.Cells[0];
		}
		else
		{
			c = ((Thing)settable).Position;
		}
		WorkGiver_Grower.wantedPlantDef = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
		if (WorkGiver_Grower.wantedPlantDef == null)
		{
			return false;
		}
		return true;
	}

	public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
	{
		Map map = pawn.Map;
		if (c.GetVacuum(pawn.Map) >= 0.5f)
		{
			return null;
		}
		if (WorkGiver_Grower.wantedPlantDef == null)
		{
			WorkGiver_Grower.wantedPlantDef = WorkGiver_Grower.CalculateWantedPlantDef(c, map);
			if (WorkGiver_Grower.wantedPlantDef == null)
			{
				return null;
			}
		}
		if (!PlantUtility.GrowthSeasonNow(c, map, WorkGiver_Grower.wantedPlantDef))
		{
			return null;
		}
		List<Thing> thingList = c.GetThingList(map);
		Zone_Growing zone_Growing = c.GetZone(map) as Zone_Growing;
		bool flag = false;
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing = thingList[i];
			if (thing.def == WorkGiver_Grower.wantedPlantDef)
			{
				return null;
			}
			if ((thing is Blueprint || thing is Frame) && thing.Faction == pawn.Faction)
			{
				flag = true;
			}
		}
		if (flag)
		{
			Thing edifice = c.GetEdifice(map);
			if (edifice == null || edifice.def.fertility < 0f)
			{
				return null;
			}
		}
		if (WorkGiver_Grower.wantedPlantDef.plant.diesToLight)
		{
			if (!c.Roofed(map) && !map.GameConditionManager.IsAlwaysDarkOutside)
			{
				JobFailReason.Is(CantSowCavePlantBecauseUnroofedTrans);
				return null;
			}
			if (map.glowGrid.GroundGlowAt(c, ignoreCavePlants: true) > 0f)
			{
				JobFailReason.Is(CantSowCavePlantBecauseOfLightTrans);
				return null;
			}
		}
		if (WorkGiver_Grower.wantedPlantDef.plant.interferesWithRoof && c.Roofed(pawn.Map))
		{
			return null;
		}
		Plant plant = c.GetPlant(map);
		if (plant != null && plant.def.plant.blockAdjacentSow)
		{
			if (!pawn.CanReserve(plant, 1, -1, null, forced) || plant.IsForbidden(pawn))
			{
				return null;
			}
			if (zone_Growing != null && !zone_Growing.allowCut)
			{
				return null;
			}
			if (!forced && plant.TryGetComp<CompPlantPreventCutting>(out var comp) && comp.PreventCutting)
			{
				return null;
			}
			if (!PlantUtility.PawnWillingToCutPlant_Job(plant, pawn))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.CutPlant, plant);
		}
		Thing thing2 = PlantUtility.AdjacentSowBlocker(WorkGiver_Grower.wantedPlantDef, c, map);
		if (thing2 != null)
		{
			if (thing2 is Plant plant2 && pawn.CanReserveAndReach(plant2, PathEndMode.Touch, Danger.Deadly, 1, -1, null, forced) && !plant2.IsForbidden(pawn))
			{
				IPlantToGrowSettable plantToGrowSettable = plant2.Position.GetPlantToGrowSettable(plant2.Map);
				if (plantToGrowSettable == null || plantToGrowSettable.GetPlantDefToGrow() != plant2.def)
				{
					Zone_Growing zone_Growing2 = c.GetZone(map) as Zone_Growing;
					Zone_Growing zone_Growing3 = plant2.Position.GetZone(map) as Zone_Growing;
					if ((zone_Growing2 != null && !zone_Growing2.allowCut) || (zone_Growing3 != null && !zone_Growing3.allowCut && plant2.def == zone_Growing3.GetPlantDefToGrow()))
					{
						return null;
					}
					if (!forced && thing2.TryGetComp(out CompPlantPreventCutting comp2) && comp2.PreventCutting)
					{
						return null;
					}
					if (PlantUtility.TreeMarkedForExtraction(plant2))
					{
						return null;
					}
					if (!PlantUtility.PawnWillingToCutPlant_Job(plant2, pawn))
					{
						return null;
					}
					return JobMaker.MakeJob(JobDefOf.CutPlant, plant2);
				}
			}
			return null;
		}
		if (WorkGiver_Grower.wantedPlantDef.plant.sowMinSkill > 0 && ((pawn.skills != null && pawn.skills.GetSkill(SkillDefOf.Plants).Level < WorkGiver_Grower.wantedPlantDef.plant.sowMinSkill) || (pawn.IsColonyMech && pawn.RaceProps.mechFixedSkillLevel < WorkGiver_Grower.wantedPlantDef.plant.sowMinSkill)))
		{
			JobFailReason.Is("UnderAllowedSkill".Translate(WorkGiver_Grower.wantedPlantDef.plant.sowMinSkill), def.label);
			return null;
		}
		for (int j = 0; j < thingList.Count; j++)
		{
			Thing thing3 = thingList[j];
			if (!thing3.def.BlocksPlanting())
			{
				continue;
			}
			if (!pawn.CanReserve(thing3, 1, -1, null, forced))
			{
				return null;
			}
			if (thing3.def.category == ThingCategory.Plant)
			{
				if (thing3.IsForbidden(pawn))
				{
					return null;
				}
				if (zone_Growing != null && !zone_Growing.allowCut)
				{
					return null;
				}
				if (!forced && plant.TryGetComp<CompPlantPreventCutting>(out var comp3) && comp3.PreventCutting)
				{
					return null;
				}
				if (!PlantUtility.PawnWillingToCutPlant_Job(thing3, pawn))
				{
					return null;
				}
				if (PlantUtility.TreeMarkedForExtraction(thing3))
				{
					return null;
				}
				return JobMaker.MakeJob(JobDefOf.CutPlant, thing3);
			}
			if (thing3.def.EverHaulable)
			{
				return HaulAIUtility.HaulAsideJobFor(pawn, thing3);
			}
			return null;
		}
		if (!WorkGiver_Grower.wantedPlantDef.CanNowPlantAt(c, map) || !PlantUtility.GrowthSeasonNow(c, map, WorkGiver_Grower.wantedPlantDef) || !pawn.CanReserve(c, 1, -1, null, forced))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Sow, c);
		job.plantDefToSow = WorkGiver_Grower.wantedPlantDef;
		return job;
	}
}
