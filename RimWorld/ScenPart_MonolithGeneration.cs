using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ScenPart_MonolithGeneration : ScenPart_DisableMapGen
{
	private MonolithGenerationMethod method = MonolithGenerationMethod.NearColonists;

	private static readonly LargeBuildingSpawnParms StructureSpawnParms = new LargeBuildingSpawnParms
	{
		minDistToEdge = 10,
		maxDistanceFromPlayerStartPosition = 20f,
		attemptSpawnLocationType = SpawnLocationType.Outdoors,
		attemptNotUnderBuildings = true,
		canSpawnOnImpassable = false
	};

	private static readonly LargeBuildingSpawnParms StructureSpawnParmsLoose = new LargeBuildingSpawnParms
	{
		minDistToEdge = 10,
		attemptSpawnLocationType = SpawnLocationType.Outdoors,
		attemptNotUnderBuildings = true,
		canSpawnOnImpassable = false
	};

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref method, "method", MonolithGenerationMethod.Disabled);
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		if (!Widgets.ButtonText(listing.GetScenPartRect(this, ScenPart.RowHeight), method.ToStringHuman()))
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (MonolithGenerationMethod value in Enum.GetValues(typeof(MonolithGenerationMethod)))
		{
			MonolithGenerationMethod localM = value;
			list.Add(new FloatMenuOption(localM.ToStringHuman(), delegate
			{
				method = localM;
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public override void PostMapGenerate(Map map)
	{
		if (method != MonolithGenerationMethod.Disabled && Find.Anomaly.GenerateMonolith && !(Find.GameInfo.startingTile != map.Tile))
		{
			if (!LargeBuildingCellFinder.TryFindCell(out var cell, map, StructureSpawnParms.ForThing(ThingDefOf.VoidMonolith), null, (IntVec3 c) => ScattererValidator_AvoidSpecialThings.IsValid(c, map), forceRecalculate: true) && !LargeBuildingCellFinder.TryFindCell(out cell, map, StructureSpawnParmsLoose.ForThing(ThingDefOf.VoidMonolith), null, (IntVec3 c) => ScattererValidator_AvoidSpecialThings.IsValid(c, map), forceRecalculate: true))
			{
				Log.Error("Failed to generate monolith.");
			}
			else
			{
				GenStep_Monolith.GenerateMonolith(cell, map);
			}
		}
	}

	public override int GetHashCode()
	{
		return base.GetHashCode() ^ method.GetHashCode();
	}
}
