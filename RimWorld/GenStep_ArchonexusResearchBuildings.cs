using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public abstract class GenStep_ArchonexusResearchBuildings : GenStep_ScattererBestFit
	{
		public override int SeedPart => 746395733;

		protected override IntVec2 Size => new IntVec2(20, 20);

		public override void Generate(Map map, GenStepParams parms)
		{
			count = 1;
			nearMapCenter = false;
			base.Generate(map, parms);
		}

		public override bool CollisionAt(IntVec3 cell, Map map)
		{
			if (cell.Roofed(map))
			{
				return true;
			}
			TerrainDef terrain = cell.GetTerrain(map);
			if (terrain != null && (terrain.IsWater || terrain.IsRoad))
			{
				return true;
			}
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.IsBuildingArtificial || (thingList[i].def.building != null && thingList[i].def.building.isNaturalRock))
				{
					return true;
				}
			}
			return false;
		}
	}
}
