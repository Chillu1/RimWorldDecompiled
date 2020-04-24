using Verse;

namespace RimWorld
{
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
				if (!GenConstruct.CanBuildOnTerrain(place, origin, map, Rot4.North) && origin.GetTerrain(map) != place)
				{
					return;
				}
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
			if (place is TerrainDef)
			{
				if (proximitySpacing != 0)
				{
					Log.ErrorOnce("Proximity spacing used for road terrain placement; not yet supported", 60936625);
				}
				TerrainDef terrainDef = map.terrainGrid.TerrainAt(position);
				TerrainDef terrainDef2 = (TerrainDef)place;
				if (terrainDef2 == TerrainDefOf.FlagstoneSandstone)
				{
					terrainDef2 = rockDef;
				}
				if (terrainDef2 == TerrainDefOf.Bridge)
				{
					if (terrainDef == TerrainDefOf.WaterDeep)
					{
						map.terrainGrid.SetTerrain(position, TerrainDefOf.WaterShallow);
					}
					if (terrainDef == TerrainDefOf.WaterOceanDeep)
					{
						map.terrainGrid.SetTerrain(position, TerrainDefOf.WaterOceanShallow);
					}
				}
				if (GenConstruct.CanBuildOnTerrain(terrainDef2, position, map, Rot4.North) && (!GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, position, map, Rot4.North) || terrainDef2 == TerrainDefOf.Bridge) && terrainDef != TerrainDefOf.Bridge)
				{
					if (terrainDef.HasTag("Road") && !terrainDef.Removable)
					{
						map.terrainGrid.SetTerrain(position, TerrainDefOf.Gravel);
					}
					map.terrainGrid.SetTerrain(position, terrainDef2);
				}
				if (position.OnEdge(map) && !map.roadInfo.roadEdgeTiles.Contains(position))
				{
					map.roadInfo.roadEdgeTiles.Add(position);
				}
			}
			else if (place is ThingDef)
			{
				if (GenConstruct.CanBuildOnTerrain(place, position, map, Rot4.North) && (proximitySpacing <= 0 || GenClosest.ClosestThing_Global(position, map.listerThings.ThingsOfDef((ThingDef)place), proximitySpacing) == null))
				{
					while (position.GetThingList(map).Count > 0)
					{
						position.GetThingList(map)[0].Destroy();
					}
					RoadDefGenStep_DryWithFallback.PlaceWorker(map, position, TerrainDefOf.Gravel);
					GenSpawn.Spawn(ThingMaker.MakeThing((ThingDef)place), position, map);
				}
			}
			else
			{
				Log.ErrorOnce($"Can't figure out how to place object {place} while building road", 10785584);
			}
		}
	}
}
