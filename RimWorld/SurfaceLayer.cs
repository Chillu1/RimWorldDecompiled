using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class SurfaceLayer : PlanetLayer
{
	private byte[] tileElevation;

	private byte[] tileHilliness;

	private byte[] tileTemperature;

	private byte[] tileRainfall;

	private byte[] tileSwampiness;

	private byte[] tilePollution;

	public byte[] tileFeature;

	private byte[] tileRoadOrigins;

	private byte[] tileRoadAdjacency;

	private byte[] tileRoadDef;

	private byte[] tileRiverOrigins;

	private byte[] tileRiverAdjacency;

	private byte[] tileRiverDef;

	private byte[] tileRiverDistances;

	private byte[] tileMutatorTiles;

	private byte[] tileMutatorDefs;

	private const int ElevationOffset = 8192;

	private const int TemperatureOffset = 300;

	private const float TemperatureMultiplier = 10f;

	public bool HasWorldData => tiles.Count > 0;

	public new SurfaceTile this[int tileID]
	{
		get
		{
			if ((uint)tileID >= base.TilesCount)
			{
				return null;
			}
			return (SurfaceTile)tiles[tileID];
		}
	}

	public new SurfaceTile this[PlanetTile tile] => this[tile.tileId];

	public SurfaceLayer()
	{
	}

	public SurfaceLayer(int layerId, PlanetLayerDef def, float radius, Vector3 origin, Vector3 viewCenter, float viewAngle, int subdivisions, float extraCameraAltitude, float backgroundWorldCameraOffset, float backgroundWorldCameraParallaxDistance)
		: base(layerId, def, radius, origin, viewCenter, viewAngle, subdivisions, extraCameraAltitude, backgroundWorldCameraOffset, backgroundWorldCameraParallaxDistance)
	{
	}

	internal override void ExposeBody()
	{
		base.ExposeBody();
		DataExposeUtility.LookByteArray(ref tileElevation, "tileElevation");
		DataExposeUtility.LookByteArray(ref tileHilliness, "tileHilliness");
		DataExposeUtility.LookByteArray(ref tileTemperature, "tileTemperature");
		DataExposeUtility.LookByteArray(ref tileRainfall, "tileRainfall");
		DataExposeUtility.LookByteArray(ref tileSwampiness, "tileSwampiness");
		DataExposeUtility.LookByteArray(ref tileFeature, "tileFeature");
		DataExposeUtility.LookByteArray(ref tilePollution, "tilePollution");
		DataExposeUtility.LookByteArray(ref tileRoadOrigins, "tileRoadOrigins");
		DataExposeUtility.LookByteArray(ref tileRoadAdjacency, "tileRoadAdjacency");
		DataExposeUtility.LookByteArray(ref tileRoadDef, "tileRoadDef");
		DataExposeUtility.LookByteArray(ref tileRiverOrigins, "tileRiverOrigins");
		DataExposeUtility.LookByteArray(ref tileRiverAdjacency, "tileRiverAdjacency");
		DataExposeUtility.LookByteArray(ref tileRiverDef, "tileRiverDef");
		DataExposeUtility.LookByteArray(ref tileRiverDistances, "tileRiverDistances");
		DataExposeUtility.LookByteArray(ref tileMutatorTiles, "tileMutatorTiles");
		DataExposeUtility.LookByteArray(ref tileMutatorDefs, "tileMutatorDefs");
	}

	protected override void TilesToRawData()
	{
		base.TilesToRawData();
		tileElevation = DataSerializeUtility.SerializeUshort(base.TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt((tiles[i].WaterCovered ? tiles[i].elevation : Mathf.Max(tiles[i].elevation, 1f)) + 8192f), 0, 65535));
		tileHilliness = DataSerializeUtility.SerializeByte(base.TilesCount, (int i) => (byte)tiles[i].hilliness);
		tileTemperature = DataSerializeUtility.SerializeUshort(base.TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt((tiles[i].temperature + 300f) * 10f), 0, 65535));
		tileRainfall = DataSerializeUtility.SerializeUshort(base.TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt(tiles[i].rainfall), 0, 65535));
		tileSwampiness = DataSerializeUtility.SerializeByte(base.TilesCount, (int i) => (byte)Mathf.Clamp(Mathf.RoundToInt(tiles[i].swampiness * 255f), 0, 255));
		tileFeature = DataSerializeUtility.SerializeUshort(base.TilesCount, (int i) => (tiles[i].feature != null) ? ((ushort)tiles[i].feature.uniqueID) : ushort.MaxValue);
		tilePollution = DataSerializeUtility.SerializeUshort(base.TilesCount, (int i) => (ushort)Mathf.Clamp(Mathf.RoundToInt(tiles[i].pollution * 65535f), 0, 65535));
		tileRiverDistances = DataSerializeUtility.SerializeByte(base.TilesCount, (int i) => (byte)this[i].riverDist);
		SerializeRoads();
		SerializeRivers();
		SerializeMutators();
	}

	private void SerializeMutators()
	{
		List<ushort> list = new List<ushort>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < base.TilesCount; i++)
		{
			List<TileMutatorDef> list3 = tiles[i].Mutators.ToList();
			for (int j = 0; j < list3.Count; j++)
			{
				list.Add(list3[j].shortHash);
				list2.Add(i);
			}
		}
		tileMutatorDefs = DataSerializeUtility.SerializeUshort(list.ToArray());
		tileMutatorTiles = DataSerializeUtility.SerializeInt(list2.ToArray());
	}

	private void SerializeRivers()
	{
		List<int> list = new List<int>();
		List<byte> list2 = new List<byte>();
		List<ushort> list3 = new List<ushort>();
		for (int i = 0; i < base.TilesCount; i++)
		{
			List<SurfaceTile.RiverLink> potentialRivers = this[i].potentialRivers;
			if (potentialRivers == null)
			{
				continue;
			}
			for (int j = 0; j < potentialRivers.Count; j++)
			{
				SurfaceTile.RiverLink riverLink = potentialRivers[j];
				if (riverLink.neighbor.tileId >= i)
				{
					int neighborId = GetNeighborId(i, riverLink.neighbor.tileId);
					if (neighborId < 0)
					{
						Log.ErrorOnce("Couldn't find valid neighbor for river piece", 81637014);
						continue;
					}
					list.Add(i);
					list2.Add((byte)neighborId);
					list3.Add(riverLink.river.shortHash);
				}
			}
		}
		tileRiverOrigins = DataSerializeUtility.SerializeInt(list.ToArray());
		tileRiverAdjacency = DataSerializeUtility.SerializeByte(list2.ToArray());
		tileRiverDef = DataSerializeUtility.SerializeUshort(list3.ToArray());
	}

	private void SerializeRoads()
	{
		List<int> list = new List<int>();
		List<byte> list2 = new List<byte>();
		List<ushort> list3 = new List<ushort>();
		for (int i = 0; i < base.TilesCount; i++)
		{
			List<SurfaceTile.RoadLink> potentialRoads = this[i].potentialRoads;
			if (potentialRoads == null)
			{
				continue;
			}
			for (int j = 0; j < potentialRoads.Count; j++)
			{
				SurfaceTile.RoadLink roadLink = potentialRoads[j];
				if (roadLink.neighbor.tileId >= i)
				{
					int neighborId = GetNeighborId(i, roadLink.neighbor.tileId);
					if (neighborId < 0)
					{
						Log.ErrorOnce("Couldn't find valid neighbor for road piece", 81637014);
						continue;
					}
					list.Add(i);
					list2.Add((byte)neighborId);
					list3.Add(roadLink.road.shortHash);
				}
			}
		}
		tileRoadOrigins = DataSerializeUtility.SerializeInt(list.ToArray());
		tileRoadAdjacency = DataSerializeUtility.SerializeByte(list2.ToArray());
		tileRoadDef = DataSerializeUtility.SerializeUshort(list3.ToArray());
	}

	protected override void RawDataToTiles()
	{
		bool num = tiles.Count == base.TilesCount;
		base.RawDataToTiles();
		if (num)
		{
			for (int i = 0; i < base.TilesCount; i++)
			{
				this[i].potentialRoads = null;
				this[i].potentialRivers = null;
			}
		}
		DataSerializeUtility.LoadUshort(tileElevation, base.TilesCount, delegate(int index, ushort data)
		{
			tiles[index].elevation = data - 8192;
		});
		DataSerializeUtility.LoadByte(tileHilliness, base.TilesCount, delegate(int index, byte data)
		{
			tiles[index].hilliness = (Hilliness)data;
		});
		DataSerializeUtility.LoadUshort(tileTemperature, base.TilesCount, delegate(int index, ushort data)
		{
			tiles[index].temperature = (float)(int)data / 10f - 300f;
		});
		DataSerializeUtility.LoadUshort(tileRainfall, base.TilesCount, delegate(int index, ushort data)
		{
			tiles[index].rainfall = (int)data;
		});
		DataSerializeUtility.LoadByte(tileSwampiness, base.TilesCount, delegate(int index, byte data)
		{
			tiles[index].swampiness = (float)(int)data / 255f;
		});
		DataSerializeUtility.LoadUshort(tilePollution, base.TilesCount, delegate(int index, ushort data)
		{
			tiles[index].pollution = (float)(int)data / 65535f;
		});
		DataSerializeUtility.LoadByte(tileRiverDistances, base.TilesCount, delegate(int tileID, byte data)
		{
			this[tileID].riverDist = data;
		});
		for (int num2 = 0; num2 < base.TilesCount; num2++)
		{
			tiles[num2].tile = new PlanetTile(num2, base.LayerID);
		}
		DeserializeRoads();
		DeserializeRivers();
		DeserializeMutators();
	}

	private void DeserializeMutators()
	{
		if (tileMutatorDefs == null || tileMutatorTiles == null)
		{
			return;
		}
		ushort[] array = DataSerializeUtility.DeserializeUshort(tileMutatorDefs);
		int[] array2 = DataSerializeUtility.DeserializeInt(tileMutatorTiles);
		int num = -1;
		for (int i = 0; i < array.Length; i++)
		{
			TileMutatorDef byShortHash = DefDatabase<TileMutatorDef>.GetByShortHash(array[i]);
			int num2 = array2[i];
			if (byShortHash != null)
			{
				if (tiles[num2].mutatorsNullable == null)
				{
					tiles[num2].mutatorsNullable = new List<TileMutatorDef>();
				}
				if (num2 != num)
				{
					tiles[num2].mutatorsNullable.Clear();
				}
				tiles[num2].mutatorsNullable.Add(byShortHash);
			}
			num = num2;
		}
	}

	private void DeserializeRivers()
	{
		if (tileRiverOrigins == null || tileRiverAdjacency == null || tileRiverDef == null)
		{
			return;
		}
		int[] array = DataSerializeUtility.DeserializeInt(tileRiverOrigins);
		byte[] array2 = DataSerializeUtility.DeserializeByte(tileRiverAdjacency);
		ushort[] array3 = DataSerializeUtility.DeserializeUshort(tileRiverDef);
		for (int i = 0; i < array.Length; i++)
		{
			PlanetTile planetTile = new PlanetTile(array[i], base.LayerID);
			PlanetTile tileNeighbor = GetTileNeighbor(planetTile, array2[i]);
			RiverDef byShortHash = DefDatabase<RiverDef>.GetByShortHash(array3[i]);
			if (byShortHash != null)
			{
				if (this[planetTile].potentialRivers == null)
				{
					this[planetTile].potentialRivers = new List<SurfaceTile.RiverLink>();
				}
				if (this[tileNeighbor].potentialRivers == null)
				{
					this[tileNeighbor].potentialRivers = new List<SurfaceTile.RiverLink>();
				}
				this[planetTile].potentialRivers.Add(new SurfaceTile.RiverLink
				{
					neighbor = tileNeighbor,
					river = byShortHash
				});
				this[tileNeighbor].potentialRivers.Add(new SurfaceTile.RiverLink
				{
					neighbor = planetTile,
					river = byShortHash
				});
			}
		}
	}

	private void DeserializeRoads()
	{
		if (tileRoadOrigins == null || tileRoadAdjacency == null || tileRoadDef == null)
		{
			return;
		}
		int[] array = DataSerializeUtility.DeserializeInt(tileRoadOrigins);
		byte[] array2 = DataSerializeUtility.DeserializeByte(tileRoadAdjacency);
		ushort[] array3 = DataSerializeUtility.DeserializeUshort(tileRoadDef);
		for (int i = 0; i < array.Length; i++)
		{
			PlanetTile planetTile = new PlanetTile(array[i], base.LayerID);
			PlanetTile tileNeighbor = GetTileNeighbor(planetTile, array2[i]);
			RoadDef byShortHash = DefDatabase<RoadDef>.GetByShortHash(array3[i]);
			if (byShortHash != null)
			{
				if (this[planetTile].potentialRoads == null)
				{
					this[planetTile].potentialRoads = new List<SurfaceTile.RoadLink>();
				}
				if (this[tileNeighbor].potentialRoads == null)
				{
					this[tileNeighbor].potentialRoads = new List<SurfaceTile.RoadLink>();
				}
				this[planetTile].potentialRoads.Add(new SurfaceTile.RoadLink
				{
					neighbor = tileNeighbor,
					road = byShortHash
				});
				this[tileNeighbor].potentialRoads.Add(new SurfaceTile.RoadLink
				{
					neighbor = planetTile,
					road = byShortHash
				});
			}
		}
	}
}
