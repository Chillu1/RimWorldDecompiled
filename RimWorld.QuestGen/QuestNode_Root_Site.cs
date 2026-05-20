using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_Site : QuestNode
{
	private SlateRef<SitePartDef> sitePartDef;

	private SlateRef<WorldObjectDef> worldObjectDef;

	private SlateRef<FactionDef> factionDef;

	private SlateRef<List<BiomeDef>> allowedBiomes;

	private SlateRef<Hilliness?> maxHilliness;

	private SlateRef<IntRange> distanceFromColonyRange;

	private SlateRef<List<LandmarkDef>> allowedLandmarks;

	private SlateRef<float?> selectLandmarkChance;

	private SlateRef<bool?> requiresLandmark;

	private SlateRef<bool?> desperateIgnoreBiome;

	private SlateRef<bool?> desperateIgnoreDistance;

	public SlateRef<bool> canBeSpace;

	public SlateRef<bool> requireSameOrAdjacentLayer = true;

	public SlateRef<List<PlanetLayerDef>> layerWhitelist;

	public SlateRef<List<PlanetLayerDef>> layerBlacklist;

	public const string SiteAlias = "site";

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		Quest quest = QuestGen.quest;
		if (!TryFindSiteTile(slate, out var tile))
		{
			Log.Error("Could not find valid site tile.");
			return;
		}
		Faction faction = ((factionDef != null) ? Find.FactionManager.FirstFactionOfDef(factionDef.GetValue(slate)) : Faction.OfAncientsHostile);
		Site site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
		{
			new SitePartDefWithParams(sitePartDef.GetValue(slate), new SitePartParams
			{
				points = slate.Get("points", 0f),
				threatPoints = slate.Get("points", 0f)
			})
		}, tile, faction, hiddenSitePartsPossible: false, null, worldObjectDef.GetValue(slate));
		slate.Set("site", site);
		quest.SpawnWorldObject(site);
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!TryFindSiteTile(slate, out var _))
		{
			return false;
		}
		return true;
	}

	protected virtual bool TryGetLayer(Slate slate, out PlanetTile source, out PlanetLayer layer)
	{
		layer = null;
		Map map = QuestGen.slate.Get<Map>("map");
		if (map != null && map.Tile.Valid)
		{
			source = map.Tile;
		}
		else if (!TileFinder.TryFindRandomPlayerTile(out source, allowCaravans: false, null, canBeSpace: true))
		{
			source = Find.WorldGrid.Surface.Tiles.RandomElement().tile;
		}
		if (Validator(source, source.Layer))
		{
			layer = source.Layer;
		}
		else
		{
			foreach (var (_, planetLayer2) in Find.WorldGrid.PlanetLayers.InRandomOrder())
			{
				if (planetLayer2 != source.Layer && Validator(source, planetLayer2))
				{
					layer = planetLayer2;
					break;
				}
			}
		}
		return layer != null;
		bool Validator(PlanetTile origin, PlanetLayer planetLayer3)
		{
			if (!canBeSpace.GetValue(slate) && planetLayer3.Def.isSpace)
			{
				return false;
			}
			List<PlanetLayerDef> value = layerWhitelist.GetValue(slate);
			List<PlanetLayerDef> value2 = layerBlacklist.GetValue(slate);
			if (!value.NullOrEmpty() && !value.Contains(planetLayer3.Def))
			{
				return false;
			}
			if (!value2.NullOrEmpty() && value2.Contains(planetLayer3.Def))
			{
				return false;
			}
			if (requireSameOrAdjacentLayer.GetValue(slate) && origin.Valid && origin.Layer != planetLayer3 && !planetLayer3.DirectConnectionTo(origin.Layer))
			{
				return false;
			}
			return true;
		}
	}

	protected virtual bool TryFindSiteTile(Slate slate, out PlanetTile tile)
	{
		if (!TryGetLayer(slate, out var source, out var layer))
		{
			tile = PlanetTile.Invalid;
			return false;
		}
		Hilliness hilliness = maxHilliness.GetValue(slate) ?? Hilliness.Mountainous;
		int trueMin = distanceFromColonyRange.GetValue(slate).TrueMin;
		int trueMax = distanceFromColonyRange.GetValue(slate).TrueMax;
		bool valueOrDefault = requiresLandmark.GetValue(slate) == true;
		float chance = selectLandmarkChance.GetValue(slate) ?? 0.5f;
		FastTileFinder.LandmarkMode landmarkMode = ((valueOrDefault || Rand.Chance(chance)) ? FastTileFinder.LandmarkMode.Required : FastTileFinder.LandmarkMode.Forbidden);
		FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(source, trueMin, trueMax, landmarkMode, reachable: true, Hilliness.Undefined, hilliness);
		int num = ((desperateIgnoreDistance.GetValue(slate) != true) ? distanceFromColonyRange.GetValue(slate).TrueMin : 0);
		float maxDistTiles = ((desperateIgnoreDistance.GetValue(slate) == true) ? float.MaxValue : ((float)distanceFromColonyRange.GetValue(slate).TrueMax));
		PlanetTile origin = source;
		float minDistTiles = num;
		bool checkBiome = (!desperateIgnoreBiome.GetValue(slate)) ?? true;
		FastTileFinder.TileQueryParams desperate = new FastTileFinder.TileQueryParams(origin, minDistTiles, maxDistTiles, FastTileFinder.LandmarkMode.Any, reachable: true, Hilliness.Undefined, hilliness, checkBiome);
		List<PlanetTile> list = layer.FastTileFinder.Query(query, allowedBiomes.GetValue(slate), allowedLandmarks.GetValue(slate), desperate);
		if (!list.Empty())
		{
			tile = list.RandomElement();
			return true;
		}
		tile = PlanetTile.Invalid;
		return false;
	}
}
