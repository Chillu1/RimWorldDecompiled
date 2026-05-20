using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class TileFinder
	{
		private static readonly List<(PlanetTile tile, int traversalDistance)> tmpTiles = new List<(PlanetTile, int)>();

		private static readonly List<PlanetTile> tmpPlayerTiles = new List<PlanetTile>();

		public static PlanetTile RandomStartingTile()
		{
			return RandomSettlementTileFor(Find.WorldGrid.Surface, Faction.OfPlayer, mustBeAutoChoosable: true);
		}

		public static PlanetTile RandomSettlementTileFor(Faction faction, bool mustBeAutoChoosable = false, Predicate<PlanetTile> extraValidator = null)
		{
			return RandomSettlementTileFor(Find.WorldGrid.Surface, faction, mustBeAutoChoosable, extraValidator);
		}

		public static PlanetTile RandomSettlementTileFor(PlanetLayer layer, Faction faction, bool mustBeAutoChoosable = false, Predicate<PlanetTile> extraValidator = null)
		{
			for (int i = 0; i < 500; i++)
			{
				if ((from _ in Enumerable.Range(0, 100)
					select Rand.Range(0, layer.TilesCount)).TryRandomElementByWeight(delegate(int x)
				{
					Tile tile2 = layer[x];
					if (!tile2.PrimaryBiome.canBuildBase || !tile2.PrimaryBiome.implemented || tile2.hilliness == Hilliness.Impassable)
					{
						return 0f;
					}
					if (mustBeAutoChoosable && !tile2.PrimaryBiome.canAutoChoose)
					{
						return 0f;
					}
					if (extraValidator != null && !extraValidator(tile2.tile))
					{
						return 0f;
					}
					float num = tile2.PrimaryBiome.settlementSelectionWeight;
					if (faction?.def.minSettlementTemperatureChanceCurve != null)
					{
						num *= faction.def.minSettlementTemperatureChanceCurve.Evaluate(GenTemperature.MinTemperatureAtTile(tile2.tile));
					}
					return num;
				}, out var result))
				{
					PlanetTile tile = layer[result].tile;
					if (IsValidTileForNewSettlement(tile))
					{
						return tile;
					}
				}
			}
			Log.Error($"Failed to find faction base tile for {faction}");
			return new PlanetTile(0, layer);
		}

		public static bool IsValidTileForNewSettlement(PlanetTile tile, StringBuilder reason = null, bool forGravship = false)
		{
			if (!tile.Valid)
			{
				return false;
			}
			Tile tile2 = Find.WorldGrid[tile];
			if (!tile2.PrimaryBiome.canBuildBase)
			{
				reason?.Append("CannotLandBiome".Translate(tile2.PrimaryBiome.LabelCap));
				return false;
			}
			if (!tile2.PrimaryBiome.implemented)
			{
				reason?.Append("BiomeNotImplemented".Translate() + ": " + tile2.PrimaryBiome.LabelCap);
				return false;
			}
			if (tile2.hilliness == Hilliness.Impassable)
			{
				reason?.Append("CannotLandImpassableMountains".Translate());
				return false;
			}
			Settlement settlement = Find.WorldObjects.SettlementBaseAt(tile);
			if (settlement != null && (!forGravship || !settlement.GravShipCanLandOn))
			{
				if (reason != null)
				{
					if (settlement.Faction == null)
					{
						reason.Append("TileOccupied".Translate());
					}
					else if (settlement.Faction == Faction.OfPlayer)
					{
						reason.Append("YourBaseAlreadyThere".Translate());
					}
					else
					{
						reason.Append("BaseAlreadyThere".Translate(settlement.Faction.Name));
					}
				}
				return false;
			}
			if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tile, out var _) && !forGravship)
			{
				reason?.Append("FactionBaseAdjacent".Translate());
				return false;
			}
			if (Find.WorldObjects.AnyWorldObjectAt<PeaceTalks>(tile))
			{
				reason?.Append("TileOccupied".Translate());
				return false;
			}
			if (forGravship)
			{
				Site site = Find.WorldObjects.SiteAt(tile);
				if (site != null && site.preventGravshipLanding)
				{
					reason?.Append("TileOccupied".Translate());
					return false;
				}
			}
			if (Find.WorldObjects.AnyWorldObjectAt<Camp>(tile))
			{
				return true;
			}
			if (!SettleInEmptyTileUtility.CanCreateMapAt(tile, forGravship))
			{
				if (forGravship)
				{
					MapParent mapParent = Find.WorldObjects.MapParentAt(tile);
					if (mapParent != null && mapParent.GravShipCanLandOn)
					{
						return true;
					}
				}
				reason?.Append("TileOccupied".Translate());
				return false;
			}
			return true;
		}

		public static bool TryFindPassableTileWithTraversalDistance(PlanetTile rootTile, int minDist, int maxDist, out PlanetTile result, Predicate<PlanetTile> validator = null, bool ignoreFirstTilePassability = false, TileFinderMode tileFinderMode = TileFinderMode.Random, bool canTraverseImpassable = false, bool exitOnFirstTileFound = false)
		{
			tmpTiles.Clear();
			rootTile.Layer.Filler.FloodFill(rootTile, (PlanetTile x) => canTraverseImpassable || !Find.World.Impassable(x) || (x == rootTile && ignoreFirstTilePassability), delegate(PlanetTile tile, int traversalDistance)
			{
				if (traversalDistance > maxDist)
				{
					return false;
				}
				if (traversalDistance >= minDist && !Find.World.Impassable(tile) && (validator == null || validator(tile)))
				{
					tmpTiles.Add((tile, traversalDistance));
					if (exitOnFirstTileFound)
					{
						return true;
					}
				}
				return false;
			});
			if (exitOnFirstTileFound && tmpTiles.Count > 0)
			{
				result = tmpTiles[0].tile;
				return true;
			}
			(PlanetTile, int) result2;
			switch (tileFinderMode)
			{
			case TileFinderMode.Near:
				if (tmpTiles.TryRandomElementByWeight<(PlanetTile, int)>(((PlanetTile tile, int traversalDistance) x) => 1f - (float)(x.traversalDistance - minDist) / ((float)(maxDist - minDist) + 0.01f), out result2))
				{
					(result, _) = result2;
					return true;
				}
				result = PlanetTile.Invalid;
				return false;
			case TileFinderMode.Furthest:
				if (tmpTiles.Count > 0)
				{
					int maxDistanceWithOffset = Mathf.Clamp(tmpTiles.MaxBy(((PlanetTile tile, int traversalDistance) t) => t.traversalDistance).traversalDistance - 2, minDist, maxDist);
					if (tmpTiles.Where(((PlanetTile tile, int traversalDistance) t) => t.traversalDistance >= maxDistanceWithOffset - 1).TryRandomElement<(PlanetTile, int)>(out var result3))
					{
						(result, _) = result3;
						return true;
					}
				}
				result = PlanetTile.Invalid;
				return false;
			case TileFinderMode.Random:
				if (tmpTiles.TryRandomElement<(PlanetTile, int)>(out result2))
				{
					(result, _) = result2;
					return true;
				}
				result = PlanetTile.Invalid;
				return false;
			default:
				Log.Error($"Unknown tile distance preference {tileFinderMode}.");
				result = PlanetTile.Invalid;
				return false;
			}
		}

		public static bool TryFindTileWithDistance(PlanetTile rootTile, int minDist, int maxDist, out PlanetTile result, Predicate<PlanetTile> validator = null, TileFinderMode tileFinderMode = TileFinderMode.Random, bool exitOnFirstTileFound = false)
		{
			tmpTiles.Clear();
			rootTile.Layer.Filler.FloodFill(rootTile, (PlanetTile _) => true, delegate(PlanetTile tile, int traversalDistance)
			{
				if (traversalDistance > maxDist)
				{
					return false;
				}
				if (traversalDistance >= minDist && (validator == null || validator(tile)))
				{
					tmpTiles.Add((tile, traversalDistance));
					if (exitOnFirstTileFound)
					{
						return true;
					}
				}
				return false;
			});
			if (exitOnFirstTileFound)
			{
				if (tmpTiles.Count > 0)
				{
					result = tmpTiles[0].tile;
					return true;
				}
				result = PlanetTile.Invalid;
				return false;
			}
			(PlanetTile, int) result2;
			switch (tileFinderMode)
			{
			case TileFinderMode.Near:
				if (tmpTiles.TryRandomElementByWeight<(PlanetTile, int)>(((PlanetTile tile, int traversalDistance) x) => 1f - (float)(x.traversalDistance - minDist) / ((float)(maxDist - minDist) + 0.01f), out result2))
				{
					(result, _) = result2;
					return true;
				}
				result = PlanetTile.Invalid;
				return false;
			case TileFinderMode.Furthest:
				if (tmpTiles.Count > 0)
				{
					int maxDistanceWithOffset = Mathf.Clamp(tmpTiles.MaxBy(((PlanetTile tile, int traversalDistance) t) => t.traversalDistance).traversalDistance - 2, minDist, maxDist);
					if (tmpTiles.Where(((PlanetTile tile, int traversalDistance) t) => t.traversalDistance >= maxDistanceWithOffset - 1).TryRandomElement<(PlanetTile, int)>(out var result3))
					{
						(result, _) = result3;
						return true;
					}
				}
				result = PlanetTile.Invalid;
				return false;
			case TileFinderMode.Random:
				if (tmpTiles.TryRandomElement<(PlanetTile, int)>(out result2))
				{
					(result, _) = result2;
					return true;
				}
				result = PlanetTile.Invalid;
				return false;
			default:
				Log.Error($"Unknown tile distance preference {tileFinderMode}.");
				result = PlanetTile.Invalid;
				return false;
			}
		}

		public static bool TryFindRandomPlayerTile(out PlanetTile tile, bool allowCaravans, Predicate<PlanetTile> validator = null, bool canBeSpace = false, PlanetLayer layer = null)
		{
			tmpPlayerTiles.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].IsPlayerHome && maps[i].mapPawns.FreeColonistsSpawnedCount != 0 && (maps[i].Tile.LayerDef.SurfaceTiles || canBeSpace) && (layer == null || maps[i].Tile.Layer == layer) && (validator == null || validator(maps[i].Tile)))
				{
					tmpPlayerTiles.Add(maps[i].Tile);
				}
			}
			if (allowCaravans)
			{
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int j = 0; j < caravans.Count; j++)
				{
					if (caravans[j].IsPlayerControlled && (maps[j].Tile.LayerDef.SurfaceTiles || canBeSpace) && (validator == null || validator(caravans[j].Tile)) && (layer == null || caravans[j].Tile.Layer == layer))
					{
						tmpPlayerTiles.Add(caravans[j].Tile);
					}
				}
			}
			if (tmpPlayerTiles.TryRandomElement(out tile))
			{
				return true;
			}
			if (Find.Maps.Where((Map x) => x.IsPlayerHome && (x.Tile.LayerDef.SurfaceTiles || canBeSpace) && (validator == null || validator(x.Tile)) && (layer == null || x.Tile.Layer == layer)).TryRandomElement(out var result))
			{
				tile = result.Tile;
				return true;
			}
			if (Find.Maps.Where((Map x) => x.mapPawns.FreeColonistsSpawnedCount != 0 && (x.Tile.LayerDef.SurfaceTiles || canBeSpace) && (validator == null || validator(x.Tile)) && (layer == null || x.Tile.Layer == layer)).TryRandomElement(out var result2))
			{
				tile = result2.Tile;
				return true;
			}
			if (!allowCaravans && Find.WorldObjects.Caravans.Where((Caravan x) => x.IsPlayerControlled && (x.Tile.LayerDef.SurfaceTiles || canBeSpace) && (validator == null || validator(x.Tile)) && (layer == null || x.Tile.Layer == layer)).TryRandomElement(out var result3))
			{
				tile = result3.Tile;
				return true;
			}
			tile = PlanetTile.Invalid;
			return false;
		}

		public static bool TryFindNewSiteTile(out PlanetTile tile, int minDist = 7, int maxDist = 27, bool allowCaravans = false, List<LandmarkDef> allowedLandmarks = null, float selectLandmarkChance = 0.5f, bool canSelectComboLandmarks = true, TileFinderMode tileFinderMode = TileFinderMode.Near, bool exitOnFirstTileFound = false, bool canBeSpace = false, PlanetLayer layer = null, Predicate<PlanetTile> validator = null)
		{
			return TryFindNewSiteTile(out tile, PlanetTile.Invalid, minDist, maxDist, allowCaravans, allowedLandmarks, selectLandmarkChance, canSelectComboLandmarks, tileFinderMode, exitOnFirstTileFound, canBeSpace, layer, validator);
		}

		public static bool TryFindNewSiteTile(out PlanetTile tile, PlanetTile nearTile, int minDist = 7, int maxDist = 27, bool allowCaravans = false, List<LandmarkDef> allowedLandmarks = null, float selectLandmarkChance = 0.5f, bool canSelectComboLandmarks = true, TileFinderMode tileFinderMode = TileFinderMode.Near, bool exitOnFirstTileFound = false, bool canBeSpace = false, PlanetLayer layer = null, Predicate<PlanetTile> validator = null)
		{
			bool flag = ModsConfig.OdysseyActive && Rand.ChanceSeeded(selectLandmarkChance, Gen.HashCombineInt(Find.TickManager.TicksGame, 18271));
			if (!nearTile.Valid && !TryFindRandomPlayerTile(out nearTile, allowCaravans, null, canBeSpace: true))
			{
				tile = PlanetTile.Invalid;
				return false;
			}
			if (layer == null)
			{
				layer = nearTile.Layer;
			}
			if (!canBeSpace && layer.Def.isSpace && !Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(nearTile, PlanetLayerDefOf.Surface, out layer))
			{
				(_, layer) = (KeyValuePair<int, PlanetLayer>)(ref Find.WorldGrid.PlanetLayers.Where((KeyValuePair<int, PlanetLayer> t) => !t.Value.Def.isSpace).RandomElement());
			}
			FastTileFinder.LandmarkMode landmarkMode = (flag ? FastTileFinder.LandmarkMode.Required : FastTileFinder.LandmarkMode.Any);
			FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(nearTile, minDist, maxDist, landmarkMode, reachable: true, Hilliness.Undefined, Hilliness.Undefined, checkBiome: true, validSettlement: true, canSelectComboLandmarks);
			List<PlanetTile> list = layer.FastTileFinder.Query(query, null, allowedLandmarks);
			if (validator != null)
			{
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					if (!validator(list[num2]))
					{
						list.RemoveAt(num2);
					}
				}
			}
			if (list.Empty())
			{
				if (TryFillFindTile(layer.GetClosestTile_NewTemp(nearTile), out tile, minDist, maxDist, allowedLandmarks, canSelectComboLandmarks, tileFinderMode, exitOnFirstTileFound, validator, flag))
				{
					return true;
				}
				tile = PlanetTile.Invalid;
				return false;
			}
			tile = list.RandomElement();
			return true;
		}

		private static bool TryFillFindTile(PlanetTile root, out PlanetTile tile, int minDist = 7, int maxDist = 27, List<LandmarkDef> allowedLandmarks = null, bool canSelectComboLandmarks = true, TileFinderMode tileFinderMode = TileFinderMode.Near, bool exitOnFirstTileFound = false, Predicate<PlanetTile> validator = null, bool pickLandmark = false)
		{
			if (TryFindPassableTileWithTraversalDistance(root, minDist, maxDist, out tile, (PlanetTile c) => ValidTile(c, pickLandmark), ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: false, exitOnFirstTileFound))
			{
				return true;
			}
			if (pickLandmark && TryFindPassableTileWithTraversalDistance(root, minDist, maxDist, out tile, (PlanetTile c) => ValidTile(c, landmark: false), ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: false, exitOnFirstTileFound))
			{
				return true;
			}
			if (TryFindPassableTileWithTraversalDistance(root, minDist, maxDist, out tile, BackupValidTile, ignoreFirstTilePassability: false, tileFinderMode, canTraverseImpassable: true, exitOnFirstTileFound))
			{
				return true;
			}
			tile = PlanetTile.Invalid;
			return false;
			bool BackupValidTile(PlanetTile x)
			{
				if (!Find.WorldObjects.AnyWorldObjectAt(x) && IsValidTileForNewSettlement(x))
				{
					Predicate<PlanetTile> predicate = validator;
					if (predicate == null || predicate(x))
					{
						if (Find.World.Impassable(x))
						{
							return Find.WorldGrid[x].WaterCovered;
						}
						return true;
					}
				}
				return false;
			}
			bool ValidTile(PlanetTile x, bool landmark)
			{
				if (ModsConfig.OdysseyActive)
				{
					Landmark landmarkHere = Find.World.landmarks[x];
					if (!landmark && landmarkHere != null)
					{
						return false;
					}
					if (landmark && (landmarkHere == null || (!allowedLandmarks.NullOrEmpty() && !allowedLandmarks.Any((LandmarkDef l) => l == landmarkHere.def))))
					{
						return false;
					}
					if (!canSelectComboLandmarks && landmarkHere != null && landmarkHere.isComboLandmark)
					{
						return false;
					}
				}
				if (!Find.WorldObjects.AnyWorldObjectAt(x) && IsValidTileForNewSettlement(x))
				{
					return validator?.Invoke(x) ?? true;
				}
				return false;
			}
		}
	}
}
