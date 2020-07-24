using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGrid : IExposable
	{
		public List<Tile> tiles = new List<Tile>();

		public List<Vector3> verts;

		public List<int> tileIDToVerts_offsets;

		public List<int> tileIDToNeighbors_offsets;

		public List<int> tileIDToNeighbors_values;

		public float averageTileSize;

		public Vector3 viewCenter;

		public float viewAngle;

		private byte[] tileBiome;

		private byte[] tileElevation;

		private byte[] tileHilliness;

		private byte[] tileTemperature;

		private byte[] tileRainfall;

		private byte[] tileSwampiness;

		public byte[] tileFeature;

		private byte[] tileRoadOrigins;

		private byte[] tileRoadAdjacency;

		private byte[] tileRoadDef;

		private byte[] tileRiverOrigins;

		private byte[] tileRiverAdjacency;

		private byte[] tileRiverDef;

		private static List<int> tmpNeighbors = new List<int>();

		private const int SubdivisionsCount = 10;

		public const float PlanetRadius = 100f;

		public const int ElevationOffset = 8192;

		public const int TemperatureOffset = 300;

		public const float TemperatureMultiplier = 10f;

		private int cachedTraversalDistance = -1;

		private int cachedTraversalDistanceForStart = -1;

		private int cachedTraversalDistanceForEnd = -1;

		public int TilesCount => tileIDToNeighbors_offsets.Count;

		public Vector3 NorthPolePos => new Vector3(0f, 100f, 0f);

		public Tile this[int tileID]
		{
			get
			{
				if ((uint)tileID >= TilesCount)
				{
					return null;
				}
				return tiles[tileID];
			}
		}

		public bool HasWorldData => tileBiome != null;

		public WorldGrid()
		{
			CalculateViewCenterAndAngle();
			PlanetShapeGenerator.Generate(10, out verts, out tileIDToVerts_offsets, out tileIDToNeighbors_offsets, out tileIDToNeighbors_values, 100f, viewCenter, viewAngle);
			CalculateAverageTileSize();
		}

		public bool InBounds(int tileID)
		{
			return (uint)tileID < TilesCount;
		}

		public Vector2 LongLatOf(int tileID)
		{
			Vector3 tileCenter = GetTileCenter(tileID);
			float x = Mathf.Atan2(tileCenter.x, 0f - tileCenter.z) * 57.29578f;
			float y = Mathf.Asin(tileCenter.y / 100f) * 57.29578f;
			return new Vector2(x, y);
		}

		public float GetHeadingFromTo(Vector3 from, Vector3 to)
		{
			if (from == to)
			{
				return 0f;
			}
			Vector3 northPolePos = NorthPolePos;
			WorldRendererUtility.GetTangentialVectorFacing(from, northPolePos, out Vector3 forward, out Vector3 right);
			WorldRendererUtility.GetTangentialVectorFacing(from, to, out Vector3 forward2, out Vector3 _);
			float num = Vector3.Angle(forward, forward2);
			if (Vector3.Dot(forward2, right) < 0f)
			{
				num = 360f - num;
			}
			return num;
		}

		public float GetHeadingFromTo(int fromTileID, int toTileID)
		{
			if (fromTileID == toTileID)
			{
				return 0f;
			}
			Vector3 tileCenter = GetTileCenter(fromTileID);
			Vector3 tileCenter2 = GetTileCenter(toTileID);
			return GetHeadingFromTo(tileCenter, tileCenter2);
		}

		public Direction8Way GetDirection8WayFromTo(int fromTileID, int toTileID)
		{
			float headingFromTo = GetHeadingFromTo(fromTileID, toTileID);
			if (headingFromTo >= 337.5f || headingFromTo < 22.5f)
			{
				return Direction8Way.North;
			}
			if (headingFromTo < 67.5f)
			{
				return Direction8Way.NorthEast;
			}
			if (headingFromTo < 112.5f)
			{
				return Direction8Way.East;
			}
			if (headingFromTo < 157.5f)
			{
				return Direction8Way.SouthEast;
			}
			if (headingFromTo < 202.5f)
			{
				return Direction8Way.South;
			}
			if (headingFromTo < 247.5f)
			{
				return Direction8Way.SouthWest;
			}
			if (headingFromTo < 292.5f)
			{
				return Direction8Way.West;
			}
			return Direction8Way.NorthWest;
		}

		public Rot4 GetRotFromTo(int fromTileID, int toTileID)
		{
			float headingFromTo = GetHeadingFromTo(fromTileID, toTileID);
			if (headingFromTo >= 315f || headingFromTo < 45f)
			{
				return Rot4.North;
			}
			if (headingFromTo < 135f)
			{
				return Rot4.East;
			}
			if (headingFromTo < 225f)
			{
				return Rot4.South;
			}
			return Rot4.West;
		}

		public void GetTileVertices(int tileID, List<Vector3> outVerts)
		{
			PackedListOfLists.GetList(tileIDToVerts_offsets, verts, tileID, outVerts);
		}

		public void GetTileVerticesIndices(int tileID, List<int> outVertsIndices)
		{
			PackedListOfLists.GetListValuesIndices(tileIDToVerts_offsets, verts, tileID, outVertsIndices);
		}

		public void GetTileNeighbors(int tileID, List<int> outNeighbors)
		{
			PackedListOfLists.GetList(tileIDToNeighbors_offsets, tileIDToNeighbors_values, tileID, outNeighbors);
		}

		public int GetTileNeighborCount(int tileID)
		{
			return PackedListOfLists.GetListCount(tileIDToNeighbors_offsets, tileIDToNeighbors_values, tileID);
		}

		public int GetMaxTileNeighborCountEver(int tileID)
		{
			return PackedListOfLists.GetListCount(tileIDToVerts_offsets, verts, tileID);
		}

		public bool IsNeighbor(int tile1, int tile2)
		{
			GetTileNeighbors(tile1, tmpNeighbors);
			return tmpNeighbors.Contains(tile2);
		}

		public bool IsNeighborOrSame(int tile1, int tile2)
		{
			if (tile1 != tile2)
			{
				return IsNeighbor(tile1, tile2);
			}
			return true;
		}

		public int GetNeighborId(int tile1, int tile2)
		{
			GetTileNeighbors(tile1, tmpNeighbors);
			return tmpNeighbors.IndexOf(tile2);
		}

		public int GetTileNeighbor(int tileID, int adjacentId)
		{
			GetTileNeighbors(tileID, tmpNeighbors);
			return tmpNeighbors[adjacentId];
		}

		public Vector3 GetTileCenter(int tileID)
		{
			int num = (tileID + 1 < tileIDToVerts_offsets.Count) ? tileIDToVerts_offsets[tileID + 1] : verts.Count;
			Vector3 zero = Vector3.zero;
			int num2 = 0;
			for (int i = tileIDToVerts_offsets[tileID]; i < num; i++)
			{
				zero += verts[i];
				num2++;
			}
			return zero / num2;
		}

		public float TileRadiusToAngle(float radius)
		{
			return DistOnSurfaceToAngle(radius * averageTileSize);
		}

		public float DistOnSurfaceToAngle(float dist)
		{
			return dist / ((float)Math.PI * 200f) * 360f;
		}

		public float DistanceFromEquatorNormalized(int tile)
		{
			return Mathf.Abs(Find.WorldGrid.GetTileCenter(tile).y / 100f);
		}

		public float ApproxDistanceInTiles(float sphericalDistance)
		{
			return sphericalDistance * 100f / averageTileSize;
		}

		public float ApproxDistanceInTiles(int firstTile, int secondTile)
		{
			Vector3 tileCenter = GetTileCenter(firstTile);
			return ApproxDistanceInTiles(GenMath.SphericalDistance(normalizedB: GetTileCenter(secondTile).normalized, normalizedA: tileCenter.normalized));
		}

		public void OverlayRoad(int fromTile, int toTile, RoadDef roadDef)
		{
			if (roadDef == null)
			{
				Log.ErrorOnce("Attempted to remove road with overlayRoad; not supported", 90292249);
				return;
			}
			RoadDef roadDef2 = GetRoadDef(fromTile, toTile, visibleOnly: false);
			if (roadDef2 == roadDef)
			{
				return;
			}
			Tile tile = this[fromTile];
			Tile tile2 = this[toTile];
			if (roadDef2 != null)
			{
				if (roadDef2.priority >= roadDef.priority)
				{
					return;
				}
				tile.potentialRoads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == toTile);
				tile2.potentialRoads.RemoveAll((Tile.RoadLink rl) => rl.neighbor == fromTile);
			}
			if (tile.potentialRoads == null)
			{
				tile.potentialRoads = new List<Tile.RoadLink>();
			}
			if (tile2.potentialRoads == null)
			{
				tile2.potentialRoads = new List<Tile.RoadLink>();
			}
			List<Tile.RoadLink> potentialRoads = tile.potentialRoads;
			Tile.RoadLink item = new Tile.RoadLink
			{
				neighbor = toTile,
				road = roadDef
			};
			potentialRoads.Add(item);
			List<Tile.RoadLink> potentialRoads2 = tile2.potentialRoads;
			item = new Tile.RoadLink
			{
				neighbor = fromTile,
				road = roadDef
			};
			potentialRoads2.Add(item);
		}

		public RoadDef GetRoadDef(int fromTile, int toTile, bool visibleOnly = true)
		{
			if (!IsNeighbor(fromTile, toTile))
			{
				Log.ErrorOnce("Tried to find road information between non-neighboring tiles", 12390444);
				return null;
			}
			Tile tile = tiles[fromTile];
			List<Tile.RoadLink> list = visibleOnly ? tile.Roads : tile.potentialRoads;
			if (list == null)
			{
				return null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].neighbor == toTile)
				{
					return list[i].road;
				}
			}
			return null;
		}

		public void OverlayRiver(int fromTile, int toTile, RiverDef riverDef)
		{
			if (riverDef == null)
			{
				Log.ErrorOnce("Attempted to remove river with overlayRiver; not supported", 90292250);
				return;
			}
			RiverDef riverDef2 = GetRiverDef(fromTile, toTile, visibleOnly: false);
			if (riverDef2 == riverDef)
			{
				return;
			}
			Tile tile = this[fromTile];
			Tile tile2 = this[toTile];
			if (riverDef2 != null)
			{
				if (riverDef2.degradeThreshold >= riverDef.degradeThreshold)
				{
					return;
				}
				tile.potentialRivers.RemoveAll((Tile.RiverLink rl) => rl.neighbor == toTile);
				tile2.potentialRivers.RemoveAll((Tile.RiverLink rl) => rl.neighbor == fromTile);
			}
			if (tile.potentialRivers == null)
			{
				tile.potentialRivers = new List<Tile.RiverLink>();
			}
			if (tile2.potentialRivers == null)
			{
				tile2.potentialRivers = new List<Tile.RiverLink>();
			}
			List<Tile.RiverLink> potentialRivers = tile.potentialRivers;
			Tile.RiverLink item = new Tile.RiverLink
			{
				neighbor = toTile,
				river = riverDef
			};
			potentialRivers.Add(item);
			List<Tile.RiverLink> potentialRivers2 = tile2.potentialRivers;
			item = new Tile.RiverLink
			{
				neighbor = fromTile,
				river = riverDef
			};
			potentialRivers2.Add(item);
		}

		public RiverDef GetRiverDef(int fromTile, int toTile, bool visibleOnly = true)
		{
			if (!IsNeighbor(fromTile, toTile))
			{
				Log.ErrorOnce("Tried to find river information between non-neighboring tiles", 12390444);
				return null;
			}
			Tile tile = tiles[fromTile];
			List<Tile.RiverLink> list = visibleOnly ? tile.Rivers : tile.potentialRivers;
			if (list == null)
			{
				return null;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].neighbor == toTile)
				{
					return list[i].river;
				}
			}
			return null;
		}

		public float GetRoadMovementDifficultyMultiplier(int fromTile, int toTile, StringBuilder explanation = null)
		{
			List<Tile.RoadLink> roads = tiles[fromTile].Roads;
			if (roads == null)
			{
				return 1f;
			}
			if (toTile == -1)
			{
				toTile = FindMostReasonableAdjacentTileForDisplayedPathCost(fromTile);
			}
			for (int i = 0; i < roads.Count; i++)
			{
				if (roads[i].neighbor != toTile)
				{
					continue;
				}
				float movementCostMultiplier = roads[i].road.movementCostMultiplier;
				if (explanation != null)
				{
					if (explanation.Length > 0)
					{
						explanation.AppendLine();
					}
					explanation.Append(roads[i].road.LabelCap + ": " + movementCostMultiplier.ToStringPercent());
				}
				return movementCostMultiplier;
			}
			return 1f;
		}

		public int FindMostReasonableAdjacentTileForDisplayedPathCost(int fromTile)
		{
			Tile tile = tiles[fromTile];
			float num = 1f;
			int num2 = -1;
			List<Tile.RoadLink> roads = tile.Roads;
			if (roads != null)
			{
				for (int i = 0; i < roads.Count; i++)
				{
					float movementCostMultiplier = roads[i].road.movementCostMultiplier;
					if (movementCostMultiplier < num && !Find.World.Impassable(roads[i].neighbor))
					{
						num = movementCostMultiplier;
						num2 = roads[i].neighbor;
					}
				}
			}
			if (num2 != -1)
			{
				return num2;
			}
			tmpNeighbors.Clear();
			GetTileNeighbors(fromTile, tmpNeighbors);
			for (int j = 0; j < tmpNeighbors.Count; j++)
			{
				if (!Find.World.Impassable(tmpNeighbors[j]))
				{
					return tmpNeighbors[j];
				}
			}
			return fromTile;
		}

		public int TraversalDistanceBetween(int start, int end, bool passImpassable = true, int maxDist = int.MaxValue)
		{
			if (start == end)
			{
				return 0;
			}
			if (start < 0 || end < 0)
			{
				return int.MaxValue;
			}
			if (cachedTraversalDistanceForStart == start && cachedTraversalDistanceForEnd == end && passImpassable && maxDist == int.MaxValue)
			{
				return cachedTraversalDistance;
			}
			if (!passImpassable && !Find.WorldReachability.CanReach(start, end))
			{
				return int.MaxValue;
			}
			int finalDist = int.MaxValue;
			int maxTilesToProcess = (maxDist == int.MaxValue) ? int.MaxValue : TilesNumWithinTraversalDistance(maxDist + 1);
			Find.WorldFloodFiller.FloodFill(start, (int x) => passImpassable || !Find.World.Impassable(x), delegate(int tile, int dist)
			{
				if (tile == end)
				{
					finalDist = dist;
					return true;
				}
				return false;
			}, maxTilesToProcess);
			if (passImpassable && maxDist == int.MaxValue)
			{
				cachedTraversalDistance = finalDist;
				cachedTraversalDistanceForStart = start;
				cachedTraversalDistanceForEnd = end;
			}
			return finalDist;
		}

		public int TilesNumWithinTraversalDistance(int traversalDist)
		{
			if (traversalDist < 0)
			{
				return 0;
			}
			return 3 * traversalDist * (traversalDist + 1) + 1;
		}

		public bool IsOnEdge(int tileID)
		{
			if (InBounds(tileID))
			{
				return GetTileNeighborCount(tileID) < GetMaxTileNeighborCountEver(tileID);
			}
			return false;
		}

		private void CalculateAverageTileSize()
		{
			int tilesCount = TilesCount;
			double num = 0.0;
			int num2 = 0;
			for (int i = 0; i < tilesCount; i++)
			{
				Vector3 tileCenter = GetTileCenter(i);
				int num3 = (i + 1 < tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_offsets[i + 1] : tileIDToNeighbors_values.Count;
				for (int j = tileIDToNeighbors_offsets[i]; j < num3; j++)
				{
					int tileID = tileIDToNeighbors_values[j];
					Vector3 tileCenter2 = GetTileCenter(tileID);
					num += (double)Vector3.Distance(tileCenter, tileCenter2);
					num2++;
				}
			}
			averageTileSize = (float)(num / (double)num2);
		}

		private void CalculateViewCenterAndAngle()
		{
			viewAngle = Find.World.PlanetCoverage * 180f;
			viewCenter = Vector3.back;
			float angle = 45f;
			if (viewAngle > 45f)
			{
				angle = Mathf.Max(90f - viewAngle, 0f);
			}
			viewCenter = Quaternion.AngleAxis(angle, Vector3.right) * viewCenter;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				TilesToRawData();
			}
			DataExposeUtility.ByteArray(ref tileBiome, "tileBiome");
			DataExposeUtility.ByteArray(ref tileElevation, "tileElevation");
			DataExposeUtility.ByteArray(ref tileHilliness, "tileHilliness");
			DataExposeUtility.ByteArray(ref tileTemperature, "tileTemperature");
			DataExposeUtility.ByteArray(ref tileRainfall, "tileRainfall");
			DataExposeUtility.ByteArray(ref tileSwampiness, "tileSwampiness");
			DataExposeUtility.ByteArray(ref tileFeature, "tileFeature");
			DataExposeUtility.ByteArray(ref tileRoadOrigins, "tileRoadOrigins");
			DataExposeUtility.ByteArray(ref tileRoadAdjacency, "tileRoadAdjacency");
			DataExposeUtility.ByteArray(ref tileRoadDef, "tileRoadDef");
			DataExposeUtility.ByteArray(ref tileRiverOrigins, "tileRiverOrigins");
			DataExposeUtility.ByteArray(ref tileRiverAdjacency, "tileRiverAdjacency");
			DataExposeUtility.ByteArray(ref tileRiverDef, "tileRiverDef");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				RawDataToTiles();
			}
		}

		public void StandardizeTileData()
		{
			TilesToRawData();
			RawDataToTiles();
		}

		private void TilesToRawData()
		{
			tileBiome = DataSerializeUtility.SerializeUshort(TilesCount, (int i) => tiles[i].biome.shortHash);
			tileElevation = DataSerializeUtility.SerializeUshort(TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt((tiles[i].WaterCovered ? tiles[i].elevation : Mathf.Max(tiles[i].elevation, 1f)) + 8192f), 0, 65535));
			tileHilliness = DataSerializeUtility.SerializeByte(TilesCount, (int i) => (byte)tiles[i].hilliness);
			tileTemperature = DataSerializeUtility.SerializeUshort(TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt((tiles[i].temperature + 300f) * 10f), 0, 65535));
			tileRainfall = DataSerializeUtility.SerializeUshort(TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt(tiles[i].rainfall), 0, 65535));
			tileSwampiness = DataSerializeUtility.SerializeByte(TilesCount, (int i) => (byte)Mathf.Clamp(Mathf.RoundToInt(tiles[i].swampiness * 255f), 0, 255));
			tileFeature = DataSerializeUtility.SerializeUshort(TilesCount, (int i) => (tiles[i].feature != null) ? ((ushort)tiles[i].feature.uniqueID) : ushort.MaxValue);
			List<int> list = new List<int>();
			List<byte> list2 = new List<byte>();
			List<ushort> list3 = new List<ushort>();
			for (int j = 0; j < TilesCount; j++)
			{
				List<Tile.RoadLink> potentialRoads = tiles[j].potentialRoads;
				if (potentialRoads == null)
				{
					continue;
				}
				for (int k = 0; k < potentialRoads.Count; k++)
				{
					Tile.RoadLink roadLink = potentialRoads[k];
					if (roadLink.neighbor >= j)
					{
						byte b = (byte)GetNeighborId(j, roadLink.neighbor);
						if (b < 0)
						{
							Log.ErrorOnce("Couldn't find valid neighbor for road piece", 81637014);
							continue;
						}
						list.Add(j);
						list2.Add(b);
						list3.Add(roadLink.road.shortHash);
					}
				}
			}
			tileRoadOrigins = DataSerializeUtility.SerializeInt(list.ToArray());
			tileRoadAdjacency = DataSerializeUtility.SerializeByte(list2.ToArray());
			tileRoadDef = DataSerializeUtility.SerializeUshort(list3.ToArray());
			List<int> list4 = new List<int>();
			List<byte> list5 = new List<byte>();
			List<ushort> list6 = new List<ushort>();
			for (int l = 0; l < TilesCount; l++)
			{
				List<Tile.RiverLink> potentialRivers = tiles[l].potentialRivers;
				if (potentialRivers == null)
				{
					continue;
				}
				for (int m = 0; m < potentialRivers.Count; m++)
				{
					Tile.RiverLink riverLink = potentialRivers[m];
					if (riverLink.neighbor >= l)
					{
						byte b2 = (byte)GetNeighborId(l, riverLink.neighbor);
						if (b2 < 0)
						{
							Log.ErrorOnce("Couldn't find valid neighbor for river piece", 81637014);
							continue;
						}
						list4.Add(l);
						list5.Add(b2);
						list6.Add(riverLink.river.shortHash);
					}
				}
			}
			tileRiverOrigins = DataSerializeUtility.SerializeInt(list4.ToArray());
			tileRiverAdjacency = DataSerializeUtility.SerializeByte(list5.ToArray());
			tileRiverDef = DataSerializeUtility.SerializeUshort(list6.ToArray());
		}

		private void RawDataToTiles()
		{
			if (tiles.Count != TilesCount)
			{
				tiles.Clear();
				for (int j = 0; j < TilesCount; j++)
				{
					tiles.Add(new Tile());
				}
			}
			else
			{
				for (int k = 0; k < TilesCount; k++)
				{
					tiles[k].potentialRoads = null;
					tiles[k].potentialRivers = null;
				}
			}
			DataSerializeUtility.LoadUshort(tileBiome, TilesCount, delegate(int i, ushort data)
			{
				tiles[i].biome = (DefDatabase<BiomeDef>.GetByShortHash(data) ?? BiomeDefOf.TemperateForest);
			});
			DataSerializeUtility.LoadUshort(tileElevation, TilesCount, delegate(int i, ushort data)
			{
				tiles[i].elevation = data - 8192;
			});
			DataSerializeUtility.LoadByte(tileHilliness, TilesCount, delegate(int i, byte data)
			{
				tiles[i].hilliness = (Hilliness)data;
			});
			DataSerializeUtility.LoadUshort(tileTemperature, TilesCount, delegate(int i, ushort data)
			{
				tiles[i].temperature = (float)(int)data / 10f - 300f;
			});
			DataSerializeUtility.LoadUshort(tileRainfall, TilesCount, delegate(int i, ushort data)
			{
				tiles[i].rainfall = (int)data;
			});
			DataSerializeUtility.LoadByte(tileSwampiness, TilesCount, delegate(int i, byte data)
			{
				tiles[i].swampiness = (float)(int)data / 255f;
			});
			int[] array = DataSerializeUtility.DeserializeInt(tileRoadOrigins);
			byte[] array2 = DataSerializeUtility.DeserializeByte(tileRoadAdjacency);
			ushort[] array3 = DataSerializeUtility.DeserializeUshort(tileRoadDef);
			for (int l = 0; l < array.Length; l++)
			{
				int num = array[l];
				int tileNeighbor = GetTileNeighbor(num, array2[l]);
				RoadDef byShortHash = DefDatabase<RoadDef>.GetByShortHash(array3[l]);
				if (byShortHash != null)
				{
					if (tiles[num].potentialRoads == null)
					{
						tiles[num].potentialRoads = new List<Tile.RoadLink>();
					}
					if (tiles[tileNeighbor].potentialRoads == null)
					{
						tiles[tileNeighbor].potentialRoads = new List<Tile.RoadLink>();
					}
					List<Tile.RoadLink> potentialRoads = tiles[num].potentialRoads;
					Tile.RoadLink item = new Tile.RoadLink
					{
						neighbor = tileNeighbor,
						road = byShortHash
					};
					potentialRoads.Add(item);
					List<Tile.RoadLink> potentialRoads2 = tiles[tileNeighbor].potentialRoads;
					item = new Tile.RoadLink
					{
						neighbor = num,
						road = byShortHash
					};
					potentialRoads2.Add(item);
				}
			}
			int[] array4 = DataSerializeUtility.DeserializeInt(tileRiverOrigins);
			byte[] array5 = DataSerializeUtility.DeserializeByte(tileRiverAdjacency);
			ushort[] array6 = DataSerializeUtility.DeserializeUshort(tileRiverDef);
			for (int m = 0; m < array4.Length; m++)
			{
				int num2 = array4[m];
				int tileNeighbor2 = GetTileNeighbor(num2, array5[m]);
				RiverDef byShortHash2 = DefDatabase<RiverDef>.GetByShortHash(array6[m]);
				if (byShortHash2 != null)
				{
					if (tiles[num2].potentialRivers == null)
					{
						tiles[num2].potentialRivers = new List<Tile.RiverLink>();
					}
					if (tiles[tileNeighbor2].potentialRivers == null)
					{
						tiles[tileNeighbor2].potentialRivers = new List<Tile.RiverLink>();
					}
					List<Tile.RiverLink> potentialRivers = tiles[num2].potentialRivers;
					Tile.RiverLink item2 = new Tile.RiverLink
					{
						neighbor = tileNeighbor2,
						river = byShortHash2
					};
					potentialRivers.Add(item2);
					List<Tile.RiverLink> potentialRivers2 = tiles[tileNeighbor2].potentialRivers;
					item2 = new Tile.RiverLink
					{
						neighbor = num2,
						river = byShortHash2
					};
					potentialRivers2.Add(item2);
				}
			}
		}
	}
}
