using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldComponent_LocationGenerator : WorldComponent
{
	private const int UpdateInterval = 90000;

	private readonly int worldLocationsTarget;

	private static readonly Dictionary<PlanetLayerDef, List<GeneratedLocationDef>> LayerList = new Dictionary<PlanetLayerDef, List<GeneratedLocationDef>>();

	private static readonly List<GeneratedLocationDef> tmpOptions = new List<GeneratedLocationDef>();

	public WorldComponent_LocationGenerator(World world)
		: base(world)
	{
		float planetCoverage = world.PlanetCoverage;
		worldLocationsTarget = ((planetCoverage < 0.301f) ? ((!(planetCoverage < 0.051f)) ? 8 : 3) : ((!(planetCoverage < 0.501f)) ? 20 : 12));
		foreach (GeneratedLocationDef item in DefDatabase<GeneratedLocationDef>.AllDefsListForReading)
		{
			foreach (PlanetLayerDef layerDef in item.LayerDefs)
			{
				if (!LayerList.TryGetValue(layerDef, out var value))
				{
					value = (LayerList[layerDef] = new List<GeneratedLocationDef>());
				}
				value.Add(item);
			}
		}
	}

	public override void WorldComponentTick()
	{
		foreach (var (_, planetLayer2) in Find.WorldGrid.PlanetLayers)
		{
			if (GenTicks.IsTickInterval(planetLayer2.GetHashCode().HashOffset(), 90000))
			{
				int num2 = Mathf.RoundToInt(planetLayer2.Def.generatedLocationFactor * (float)worldLocationsTarget);
				if (planetLayer2.Tiles.Count((Tile tile) => world.worldObjects.AnyGeneratedWorldLocationAt(tile.tile)) < num2)
				{
					GenerateUntilTarget(planetLayer2);
				}
			}
		}
	}

	private void GenerateUntilTarget()
	{
		foreach (var (_, layer) in Find.WorldGrid.PlanetLayers)
		{
			GenerateUntilTarget(layer);
		}
	}

	private void GenerateUntilTarget(PlanetLayer layer)
	{
		int num = Mathf.RoundToInt(layer.Def.generatedLocationFactor * (float)worldLocationsTarget);
		for (int i = 0; i < num; i++)
		{
			GenerateLocationForLayer(layer);
		}
	}

	private void GenerateLocationForLayer(PlanetLayer layer)
	{
		if (!LayerList.ContainsKey(layer.Def) || LayerList[layer.Def].Empty() || !TryFindSiteTile(layer, out var tile))
		{
			return;
		}
		tmpOptions.Clear();
		tmpOptions.AddRange(LayerList[layer.Def]);
		foreach (GeneratedLocationDef location in LayerList[layer.Def])
		{
			if (location.layerMaximum >= 0 && layer.Tiles.Count((Tile t) => world.worldObjects.WorldObjectOfDefAt(location.worldObjectDef, t.tile) != null) >= location.layerMaximum)
			{
				tmpOptions.Remove(location);
			}
		}
		if (!tmpOptions.Empty())
		{
			GeneratedLocationDef generatedLocationDef = tmpOptions.RandomElementByWeight((GeneratedLocationDef def) => def.weight);
			WorldObject worldObject = WorldObjectMaker.MakeWorldObject(generatedLocationDef.worldObjectDef);
			if (worldObject is INameableWorldObject nameableWorldObject)
			{
				nameableWorldObject.Name = NameGenerator.GenerateName(worldObject.def.nameMaker, null, appendNumberIfNameUsed: false, "r_name", null, null);
			}
			if (worldObject is IResourceWorldObject resourceWorldObject && !generatedLocationDef.preciousResources.NullOrEmpty())
			{
				resourceWorldObject.PreciousResource = generatedLocationDef.preciousResources.RandomElement();
			}
			if (worldObject is IExpirableWorldObject expirableWorldObject && !generatedLocationDef.TimeoutRangeDays.IsZeros)
			{
				expirableWorldObject.ExpireAtTicks = GenTicks.TicksGame + (int)(generatedLocationDef.TimeoutRangeDays.RandomInRange * 60000f);
			}
			worldObject.isGeneratedLocation = true;
			worldObject.Tile = tile;
			world.worldObjects.Add(worldObject);
		}
	}

	public static bool TryFindSiteTile(PlanetLayer layer, out PlanetTile tile)
	{
		tile = PlanetTile.Invalid;
		PlanetTile other = new PlanetTile(0, Find.WorldGrid.Surface);
		if (TileFinder.TryFindRandomPlayerTile(out var tile2, allowCaravans: false))
		{
			other = tile2;
		}
		return TileFinder.TryFindTileWithDistance(layer.GetClosestTile_NewTemp(other, validSettlement: true), 0, int.MaxValue, out tile, Validator);
		static bool Validator(PlanetTile x)
		{
			return !Find.WorldObjects.AnyWorldObjectAt(x);
		}
	}

	public override void FinalizeInit(bool fromLoad)
	{
		if (!fromLoad)
		{
			GenerateUntilTarget();
		}
	}
}
