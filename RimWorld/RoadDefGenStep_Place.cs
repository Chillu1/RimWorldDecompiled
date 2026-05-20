using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoadDefGenStep_Place : RoadDefGenStep_Bulldoze
{
	public BuildableDef place;

	public int proximitySpacing;

	public bool onlyIfOriginAllows;

	public string suppressOnTerrainTag;

	public override void Place(Map map, IntVec3 position, TerrainDef rockDef, IntVec3 origin, GenStep_Roads.DistanceElement[,] distance)
	{
		if (onlyIfOriginAllows)
		{
			bool flag = false;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = position + GenAdj.CardinalDirections[i];
				if (c.InBounds(map) && chancePerPositionCurve.Evaluate(distance[c.x, c.z].fromRoad) > 0f && (GenConstruct.CanBuildOnTerrain(place, c, map, Rot4.North) || c.GetTerrain(map) == place) && (GenConstruct.CanBuildOnTerrain(place, distance[c.x, c.z].origin, map, Rot4.North) || distance[c.x, c.z].origin.GetTerrain(map) == place))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
		}
		if (!suppressOnTerrainTag.NullOrEmpty() && map.terrainGrid.TerrainAt(position).HasTag(suppressOnTerrainTag))
		{
			return;
		}
		base.Place(map, position, rockDef, origin, distance);
		TerrainDef terrainDef = place as TerrainDef;
		if (terrainDef != null)
		{
			if (proximitySpacing != 0)
			{
				Log.ErrorOnce("Proximity spacing used for road terrain placement; not yet supported", 60936625);
			}
			TerrainDef terrainDef2 = map.terrainGrid.TerrainAt(position);
			if (terrainDef == TerrainDefOf.FlagstoneSandstone)
			{
				terrainDef = rockDef;
			}
			if (terrainDef.bridge)
			{
				if (terrainDef2 == TerrainDefOf.WaterDeep)
				{
					map.terrainGrid.SetTerrain(position, TerrainDefOf.WaterShallow);
				}
				if (terrainDef2 == TerrainDefOf.WaterOceanDeep)
				{
					map.terrainGrid.SetTerrain(position, TerrainDefOf.WaterOceanShallow);
				}
			}
			if (GenConstruct.CanBuildOnTerrain(terrainDef, position, map, Rot4.North) && (!GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, position, map, Rot4.North) || terrainDef.bridge) && !terrainDef2.bridge)
			{
				if (terrainDef2.HasTag("Road") && !terrainDef2.Removable)
				{
					map.terrainGrid.SetTerrain(position, TerrainDefOf.Gravel);
				}
				if (terrainDef.isFoundation)
				{
					map.terrainGrid.SetFoundation(position, terrainDef);
				}
				else
				{
					map.terrainGrid.SetTerrain(position, terrainDef);
				}
			}
			if (position.OnEdge(map) && !map.roadInfo.roadEdgeTiles.Contains(position))
			{
				map.roadInfo.roadEdgeTiles.Add(position);
			}
		}
		else if (place is ThingDef)
		{
			if (!GenConstruct.CanBuildOnTerrain(place, position, map, Rot4.North) || (proximitySpacing > 0 && GenClosest.ClosestThing_Global(position, map.listerThings.ThingsOfDef((ThingDef)place), proximitySpacing) != null))
			{
				return;
			}
			List<Thing> thingList = position.GetThingList(map);
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				if (!thingList[num].def.destroyable)
				{
					return;
				}
				thingList[num].Destroy();
			}
			RoadDefGenStep_DryWithFallback.PlaceWorker(map, position, TerrainDefOf.Gravel);
			GenSpawn.Spawn(ThingMaker.MakeThing((ThingDef)place), position, map);
		}
		else
		{
			Log.ErrorOnce($"Can't figure out how to place object {place} while building road", 10785584);
		}
	}
}
