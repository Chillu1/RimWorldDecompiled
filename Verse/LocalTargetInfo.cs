using System;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public struct LocalTargetInfo : IEquatable<LocalTargetInfo>
{
	private Thing thingInt;

	private IntVec3 cellInt;

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

	public Pawn Pawn => Thing as Pawn;

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

	public bool ThingDiscarded
	{
		get
		{
			if (Thing != null)
			{
				return Thing.Discarded;
			}
			return false;
		}
	}

	public static LocalTargetInfo Invalid => new LocalTargetInfo(IntVec3.Invalid);

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

	public Vector3 CenterVector3
	{
		get
		{
			if (thingInt != null)
			{
				if (thingInt.Spawned)
				{
					return thingInt.DrawPos;
				}
				if (thingInt.SpawnedOrAnyParentSpawned)
				{
					return thingInt.PositionHeld.ToVector3Shifted();
				}
				return thingInt.Position.ToVector3Shifted();
			}
			if (cellInt.IsValid)
			{
				return cellInt.ToVector3Shifted();
			}
			return default(Vector3);
		}
	}

	public LocalTargetInfo(Thing thing)
	{
		thingInt = thing;
		cellInt = IntVec3.Invalid;
	}

	public LocalTargetInfo(IntVec3 cell)
	{
		thingInt = null;
		cellInt = cell;
	}

	public bool TryGetPawn(out Pawn pawn)
	{
		pawn = Pawn;
		return pawn != null;
	}

	public static implicit operator LocalTargetInfo(Thing t)
	{
		return new LocalTargetInfo(t);
	}

	public static implicit operator LocalTargetInfo(IntVec3 c)
	{
		return new LocalTargetInfo(c);
	}

	public static explicit operator IntVec3(LocalTargetInfo targ)
	{
		if (targ.thingInt != null)
		{
			Log.ErrorOnce("Casted LocalTargetInfo to IntVec3 but it had Thing " + targ.thingInt, 6324165);
		}
		return targ.Cell;
	}

	public static explicit operator Thing(LocalTargetInfo targ)
	{
		if (targ.cellInt.IsValid)
		{
			IntVec3 intVec = targ.cellInt;
			Log.ErrorOnce("Casted LocalTargetInfo to Thing but it had cell " + intVec.ToString(), 631672);
		}
		return targ.thingInt;
	}

	public TargetInfo ToTargetInfo(Map map)
	{
		if (!IsValid)
		{
			return TargetInfo.Invalid;
		}
		if (Thing != null)
		{
			return new TargetInfo(Thing);
		}
		return new TargetInfo(Cell, map);
	}

	public GlobalTargetInfo ToGlobalTargetInfo(Map map)
	{
		if (!IsValid)
		{
			return GlobalTargetInfo.Invalid;
		}
		if (Thing != null)
		{
			return new GlobalTargetInfo(Thing);
		}
		return new GlobalTargetInfo(Cell, map);
	}

	public static bool operator ==(LocalTargetInfo a, LocalTargetInfo b)
	{
		if (a.thingInt != null || b.thingInt != null)
		{
			return a.thingInt == b.thingInt;
		}
		if (a.cellInt.IsValid || b.cellInt.IsValid)
		{
			return a.cellInt == b.cellInt;
		}
		return true;
	}

	public static bool operator !=(LocalTargetInfo a, LocalTargetInfo b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is LocalTargetInfo))
		{
			return false;
		}
		return Equals((LocalTargetInfo)obj);
	}

	public bool Equals(LocalTargetInfo other)
	{
		return this == other;
	}

	public override int GetHashCode()
	{
		if (thingInt != null)
		{
			return thingInt.GetHashCode();
		}
		return cellInt.GetHashCode();
	}

	public override string ToString()
	{
		if (Thing != null)
		{
			return Thing.GetUniqueLoadID();
		}
		if (Cell.IsValid)
		{
			return Cell.ToString();
		}
		return "null";
	}
}
