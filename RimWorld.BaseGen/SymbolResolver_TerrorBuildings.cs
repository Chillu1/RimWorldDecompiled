using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_TerrorBuildings : SymbolResolver
{
	private static readonly SimpleCurve IndoorBuildingCountCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(40f, 1f),
		new CurvePoint(120f, 2f)
	};

	private static readonly SimpleCurve OutdoorsBuildingCountCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(30f, 2f),
		new CurvePoint(80f, 3f),
		new CurvePoint(120f, 4f)
	};

	public static IEnumerable<ThingDef> TerrorBuildings => DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.StatBaseDefined(StatDefOf.TerrorSource) && typeof(Building_Casket).IsAssignableFrom(def.thingClass));

	public static bool FactionShouldHaveTerrorBuildings(Faction faction)
	{
		if (ModsConfig.IdeologyActive && faction != null && faction.ideos != null && faction.ideos.PrimaryIdeo != null)
		{
			return faction.ideos.PrimaryIdeo.IdeoApprovesOfSlavery();
		}
		return false;
	}

	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (!FactionShouldHaveTerrorBuildings(rp.faction))
		{
			return false;
		}
		return true;
	}

	private void SpawnBuildings(List<IntVec3> potentialSpots, int buildingCount, ResolveParams rp, List<IntVec3> usedSpots, List<ThingDef> usedThingDefs)
	{
		while (potentialSpots.Count > 0 && buildingCount > 0)
		{
			IntVec3 intVec = potentialSpots.MaxBy(delegate(IntVec3 pSpot)
			{
				float num = float.PositiveInfinity;
				foreach (IntVec3 usedSpot in usedSpots)
				{
					float num2 = usedSpot.DistanceTo(pSpot);
					if (num > num2)
					{
						num = num2;
					}
				}
				return num;
			});
			potentialSpots.Remove(intVec);
			usedSpots.Add(intVec);
			ThingDef thingDef = TerrorBuildings.RandomElementByWeight(delegate(ThingDef def)
			{
				int num = usedThingDefs.Count((ThingDef d) => d == def);
				return (num == 0) ? 1f : (1f / (float)num);
			});
			Building_Casket building_Casket = (Building_Casket)ThingMaker.MakeThing(thingDef, BaseGenUtility.CheapStuffFor(thingDef, rp.faction));
			Faction faction = (from f in Find.FactionManager.GetFactions(allowHidden: false, allowDefeated: false, allowNonHumanlike: false)
				where !f.IsPlayer && f.HostileTo(rp.faction)
				select f).RandomElementWithFallback(rp.faction);
			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Slave, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: false));
			pawn.Kill(null, null);
			building_Casket.TryAcceptThing(pawn.Corpse);
			ResolveParams resolveParams = rp;
			resolveParams.singleThingToSpawn = building_Casket;
			resolveParams.rect = CellRect.CenteredOn(intVec, building_Casket.def.size.x, building_Casket.def.size.z);
			BaseGen.symbolStack.Push("thing", resolveParams);
			buildingCount--;
		}
	}

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		int buildingCount = (int)IndoorBuildingCountCurve.Evaluate(rp.rect.Area);
		int buildingCount2 = (int)OutdoorsBuildingCountCurve.Evaluate(rp.rect.Area);
		List<IntVec3> list = new List<IntVec3>();
		List<IntVec3> list2 = new List<IntVec3>();
		List<IntVec3> usedSpots = new List<IntVec3>();
		List<ThingDef> usedThingDefs = new List<ThingDef>();
		foreach (IntVec3 item in rp.rect)
		{
			if (!item.InBounds(map) || !item.Standable(map) || item.GetDoor(map) != null)
			{
				continue;
			}
			int num = 0;
			bool flag = false;
			IntVec3[] adjacentCells = GenAdj.AdjacentCells;
			foreach (IntVec3 intVec in adjacentCells)
			{
				IntVec3 c = item + intVec;
				if (!c.InBounds(map))
				{
					continue;
				}
				foreach (Thing thing in c.GetThingList(map))
				{
					if (thing.def.IsEdifice())
					{
						if (thing.def != ThingDefOf.Wall)
						{
							flag = true;
							break;
						}
						num++;
					}
				}
			}
			if (!flag && (num <= 0 || num % 2 != 0))
			{
				if (item.Roofed(map))
				{
					list.Add(item);
				}
				else
				{
					list2.Add(item);
				}
			}
		}
		SpawnBuildings(list, buildingCount, rp, usedSpots, usedThingDefs);
		SpawnBuildings(list2, buildingCount2, rp, usedSpots, usedThingDefs);
	}
}
