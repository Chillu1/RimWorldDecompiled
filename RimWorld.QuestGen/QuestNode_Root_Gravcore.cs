using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public abstract class QuestNode_Root_Gravcore : QuestNode
{
	protected List<BiomeDef> allowedBiomes = new List<BiomeDef>();

	private Hilliness minHilliness;

	private Hilliness maxHilliness;

	protected IntRange distanceFromColonyRange = new IntRange(20, 60);

	protected List<LandmarkDef> allowedLandmarks;

	protected float selectLandmarkChance = 0.5f;

	public int requiredSubquestsGiven;

	private bool requiresLandmark;

	protected PlanetLayerDef layer;

	protected const string SiteAlias = "site";

	protected virtual bool TryFindSiteTile(out PlanetTile tile)
	{
		PlanetLayer planetLayer = null;
		if (!TileFinder.TryFindRandomPlayerTile(out var tile2, allowCaravans: false, null, canBeSpace: true))
		{
			Log.Error("Failed to find a valid root tile for gravcore site.");
			tile = PlanetTile.Invalid;
			return false;
		}
		if (layer != null && !Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(tile2, layer, out planetLayer))
		{
			tile = PlanetTile.Invalid;
			return false;
		}
		if (planetLayer == null)
		{
			planetLayer = tile2.Layer;
		}
		int trueMin = distanceFromColonyRange.TrueMin;
		int trueMax = distanceFromColonyRange.TrueMax;
		FastTileFinder.LandmarkMode landmarkMode = ((requiresLandmark || Rand.Chance(selectLandmarkChance)) ? FastTileFinder.LandmarkMode.Required : FastTileFinder.LandmarkMode.Forbidden);
		FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(tile2, trueMin, trueMax, landmarkMode, reachable: true, minHilliness, maxHilliness);
		FastTileFinder.TileQueryParams desperate = new FastTileFinder.TileQueryParams(tile2, 1f, trueMax * 2, FastTileFinder.LandmarkMode.Any, reachable: true, minHilliness, maxHilliness, checkBiome: false);
		List<PlanetTile> list = planetLayer.FastTileFinder.Query(query, allowedBiomes, allowedLandmarks, desperate);
		if (!list.Empty())
		{
			tile = list.RandomElement();
			return true;
		}
		query = new FastTileFinder.TileQueryParams(tile2, trueMin, trueMax, landmarkMode, reachable: true, minHilliness, maxHilliness);
		desperate = new FastTileFinder.TileQueryParams(tile2, 1f, float.MaxValue, FastTileFinder.LandmarkMode.Any, reachable: true, minHilliness, maxHilliness, checkBiome: false);
		list = planetLayer.FastTileFinder.Query(query, allowedBiomes, allowedLandmarks, desperate);
		if (!list.Empty())
		{
			tile = list.RandomElement();
			return true;
		}
		tile = PlanetTile.Invalid;
		return false;
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return false;
		}
		if (!TryFindSiteTile(out var _))
		{
			return false;
		}
		return true;
	}
}
