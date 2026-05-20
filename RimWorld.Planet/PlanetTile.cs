using System;
using Verse;

namespace RimWorld.Planet;

public readonly struct PlanetTile : IEquatable<PlanetTile>
{
	public readonly int tileId;

	private readonly int layerId;

	public static readonly PlanetTile Invalid = new PlanetTile(-1);

	public Tile Tile => Layer[tileId];

	public PlanetLayer Layer
	{
		get
		{
			if (layerId < 0)
			{
				return Find.WorldGrid.Surface;
			}
			return Find.WorldGrid.PlanetLayers[layerId];
		}
	}

	public PlanetLayerDef LayerDef => Layer.Def;

	public bool Valid => tileId >= 0;

	public PlanetTile(int tileId, PlanetLayer layer)
		: this(tileId, layer.LayerID)
	{
	}

	public PlanetTile(int tileId)
	{
		this.tileId = tileId;
		layerId = 0;
	}

	public PlanetTile(int tileId, int layerId)
	{
		this.tileId = tileId;
		this.layerId = layerId;
	}

	public bool Equals(PlanetTile other)
	{
		if (tileId != other.tileId)
		{
			return false;
		}
		if (layerId == other.layerId)
		{
			return true;
		}
		bool num = layerId < 0 || Layer.IsRootSurface;
		bool flag = other.layerId < 0 || other.Layer.IsRootSurface;
		return num && flag;
	}

	public override bool Equals(object obj)
	{
		if (obj is PlanetTile other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		bool flag = layerId < 0 || Layer.IsRootSurface;
		return (tileId * 397) ^ ((!flag) ? layerId : 0);
	}

	public static bool operator ==(PlanetTile lhs, PlanetTile rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(PlanetTile lhs, PlanetTile rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator int(PlanetTile tile)
	{
		return tile.tileId;
	}

	public static implicit operator PlanetTile(int tileId)
	{
		return new PlanetTile(tileId);
	}

	public static bool TryParse(string str, out PlanetTile tile)
	{
		string[] array = str.Split(',');
		tile = Invalid;
		int result2;
		if (array.Length == 1 && int.TryParse(array[0], out var result))
		{
			tile = new PlanetTile(result);
		}
		else if (array.Length == 2 && int.TryParse(array[0], out result) && int.TryParse(array[1], out result2))
		{
			tile = new PlanetTile(result, result2);
		}
		return tile.Valid;
	}

	public static PlanetTile FromString(string str)
	{
		if (TryParse(str, out var tile))
		{
			return tile;
		}
		return Invalid;
	}

	public override string ToString()
	{
		return $"{tileId},{layerId}";
	}
}
