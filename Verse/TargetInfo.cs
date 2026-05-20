using System;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public struct TargetInfo : IEquatable<TargetInfo>
{
	private Thing thingInt;

	private IntVec3 cellInt;

	private Map mapInt;

	public bool IsValid
	{
		get
		{
			if (thingInt == null)
			{
				return cellInt.IsValid;
			}
			return true;
		}
	}

	public bool HasThing => Thing != null;

	public Thing Thing => thingInt;

	public bool ThingDestroyed
	{
		get
		{
			if (Thing != null)
			{
				return Thing.Destroyed;
			}
			return false;
		}
	}

	public static TargetInfo Invalid => new TargetInfo(IntVec3.Invalid, null);

	public bool Fogged
	{
		get
		{
			if (!HasThing)
			{
				return cellInt.Fogged(mapInt);
			}
			return thingInt.Fogged();
		}
	}

	public string Label
	{
		get
		{
			if (thingInt != null)
			{
				return thingInt.LabelShort;
			}
			return "Location".Translate();
		}
	}

	public IntVec3 Cell
	{
		get
		{
			if (thingInt != null)
			{
				return thingInt.PositionHeld;
			}
			return cellInt;
		}
	}

	public IntVec3 CenterCell
	{
		get
		{
			if (thingInt != null)
			{
				return thingInt.OccupiedRect().CenterCell;
			}
			return cellInt;
		}
	}

	public PlanetTile Tile
	{
		get
		{
			if (thingInt != null && thingInt.Tile.Valid)
			{
				return thingInt.Tile;
			}
			if (cellInt.IsValid && mapInt != null)
			{
				return mapInt.Tile;
			}
			return PlanetTile.Invalid;
		}
	}

	public Vector3 CenterVector3 => ((LocalTargetInfo)this).CenterVector3;

	public Map Map
	{
		get
		{
			if (thingInt != null)
			{
				return thingInt.MapHeld;
			}
			return mapInt;
		}
	}

	public TargetInfo(Thing thing)
	{
		thingInt = thing;
		cellInt = IntVec3.Invalid;
		mapInt = null;
	}

	public TargetInfo(IntVec3 cell, Map map, bool allowNullMap = false)
	{
		if (!allowNullMap && cell.IsValid && map == null)
		{
			IntVec3 intVec = cell;
			Log.Warning("Constructed TargetInfo with cell=" + intVec.ToString() + " and a null map.");
		}
		thingInt = null;
		cellInt = cell;
		mapInt = map;
	}

	public static implicit operator TargetInfo(Thing t)
	{
		return new TargetInfo(t);
	}

	public static explicit operator LocalTargetInfo(TargetInfo t)
	{
		if (t.HasThing)
		{
			return new LocalTargetInfo(t.Thing);
		}
		return new LocalTargetInfo(t.Cell);
	}

	public static explicit operator IntVec3(TargetInfo targ)
	{
		if (targ.thingInt != null)
		{
			Log.ErrorOnce("Casted TargetInfo to IntVec3 but it had Thing " + targ.thingInt, 6324165);
		}
		return targ.Cell;
	}

	public static explicit operator Thing(TargetInfo targ)
	{
		if (targ.cellInt.IsValid)
		{
			IntVec3 intVec = targ.cellInt;
			Log.ErrorOnce("Casted TargetInfo to Thing but it had cell " + intVec.ToString(), 631672);
		}
		return targ.thingInt;
	}

	public static bool operator ==(TargetInfo a, TargetInfo b)
	{
		if (a.Thing != null || b.Thing != null)
		{
			return a.Thing == b.Thing;
		}
		if (a.cellInt.IsValid || b.cellInt.IsValid)
		{
			if (a.cellInt == b.cellInt)
			{
				return a.mapInt == b.mapInt;
			}
			return false;
		}
		return true;
	}

	public static bool operator !=(TargetInfo a, TargetInfo b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TargetInfo))
		{
			return false;
		}
		return Equals((TargetInfo)obj);
	}

	public bool Equals(TargetInfo other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		if (thingInt != null)
		{
			return thingInt.GetHashCode();
		}
		return Gen.HashCombine(cellInt.GetHashCode(), mapInt);
	}

	public override string ToString()
	{
		if (Thing != null)
		{
			return Thing.GetUniqueLoadID();
		}
		if (Cell.IsValid)
		{
			return Cell.ToString() + ", " + ((mapInt != null) ? mapInt.GetUniqueLoadID() : "null");
		}
		return "null";
	}
}
