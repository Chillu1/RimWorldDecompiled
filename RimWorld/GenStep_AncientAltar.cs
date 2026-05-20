using System.Collections.Generic;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class GenStep_AncientAltar : GenStep_ScattererBestFit
{
	public override int SeedPart => 572334943;

	protected override IntVec2 Size => SymbolResolver_AncientAltar.Size;

	public override void Generate(Map map, GenStepParams parms)
	{
		count = 1;
		warnOnFail = false;
		base.Generate(map, parms);
	}

	public override bool CollisionAt(IntVec3 cell, Map map)
	{
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

	protected override bool TryFindScatterCell(Map map, out IntVec3 result)
	{
		if (!base.TryFindScatterCell(map, out result))
		{
			result = map.Center;
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
	{
		SitePartParams parms2 = parms.sitePart.parms;
		ResolveParams resolveParams = new ResolveParams
		{
			triggerSecuritySignal = parms2.triggerSecuritySignal,
			threatPoints = parms2.threatPoints,
			relicThing = parms2.relicThing,
			interiorThreatPoints = ((parms2.interiorThreatPoints > 0f) ? new float?(parms2.interiorThreatPoints) : ((float?)null)),
			exteriorThreatPoints = ((parms2.exteriorThreatPoints > 0f) ? new float?(parms2.exteriorThreatPoints) : ((float?)null)),
			rect = CellRect.CenteredOn(c, Size.x, Size.z)
		};
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("ancientAltar", resolveParams);
		RimWorld.BaseGen.BaseGen.Generate();
		MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects").Add(resolveParams.rect);
		parms.sitePart.relicWasSpawned = true;
	}
}
