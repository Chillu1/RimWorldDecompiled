using System.Collections.Generic;
using RimWorld.BaseGen;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class GenStep_ConditionCauser : GenStep_Scatterer
{
	private const int Size = 10;

	private GenStepParams currentParams;

	public override int SeedPart => 1068345639;

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (!base.CanScatterAt(c, map))
		{
			return false;
		}
		CellRect rect = CellRect.CenteredOn(c, 10, 10);
		if (!rect.InBounds(map))
		{
			return false;
		}
		if (MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var var) && var.Any((CellRect x) => rect.Overlaps(x)))
		{
			return false;
		}
		foreach (IntVec3 cell in rect.Cells)
		{
			if (!GenConstruct.CanBuildOnTerrain(currentParams.sitePart.conditionCauser.def, cell, map, Rot4.North))
			{
				return false;
			}
			foreach (Thing thing in cell.GetThingList(map))
			{
				if (thing.def.building != null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		currentParams = parms;
		count = 1;
		base.Generate(map, parms);
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
	{
		Faction faction = ((map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction());
		List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		CellRect cellRect = CellRect.CenteredOn(loc, 10, 10).ClipInsideMap(map);
		SitePart sitePart = currentParams.sitePart;
		sitePart.conditionCauserWasSpawned = true;
		ResolveParams resolveParams = new ResolveParams
		{
			rect = cellRect,
			faction = faction,
			conditionCauser = sitePart.conditionCauser
		};
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("conditionCauserRoom", resolveParams);
		RimWorld.BaseGen.BaseGen.Generate();
		MapGenerator.SetVar("RectOfInterest", cellRect);
		orGenerateVar.Add(cellRect);
	}
}
