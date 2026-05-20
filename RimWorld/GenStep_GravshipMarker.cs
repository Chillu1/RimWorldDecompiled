using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class GenStep_GravshipMarker : GenStep
{
	public const int ClearEdgeRadius = 2;

	public override int SeedPart => 2031711911;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (ModsConfig.OdysseyActive)
		{
			Gravship gravship = parms.gravship;
			if (gravship != null)
			{
				IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;
				HashSet<IntVec3> cellsAdjacentToSubstructure = GravshipPlacementUtility.GetCellsAdjacentToSubstructure(gravship.OccupiedRects, 2);
				GravshipPlacementUtility.ClearArea(map, playerStartSpot, cellsAdjacentToSubstructure, GravshipPlacementUtility.ClearMode.BlockingBuildingsOnly);
				GravshipLandingMarker obj = ThingMaker.MakeThing(ThingDefOf.GravshipLandingMarker) as GravshipLandingMarker;
				obj.gravship = gravship;
				GenSpawn.Spawn(obj, playerStartSpot, map);
			}
		}
	}
}
