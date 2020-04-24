using System;
using Verse;

namespace RimWorld.Planet
{
	public struct GlobalTargetInfo : IEquatable<GlobalTargetInfo>
	{
		private Thing thingInt;

		private IntVec3 cellInt;

		private Map mapInt;

		private WorldObject worldObjectInt;

		private int tileInt;

		public const char WorldObjectLoadIDMarker = '@';

		public bool IsValid
		{
			get
			{
				if (thingInt == null && !cellInt.IsValid && worldObjectInt == null)
				{
					return tileInt >= 0;
				}
				return true;
			}
		}

		public bool IsMapTarget
		{
			get
			{
				if (!HasThing)
				{
					return cellInt.IsValid;
				}
				return true;
			}
		}

		public bool IsWorldTarget
		{
			get
			{
				if (!HasWorldObject)
				{
					return tileInt >= 0;
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

		public bool HasWorldObject => WorldObject != null;

		public WorldObject WorldObject => worldObjectInt;

		public static GlobalTargetInfo Invalid => new GlobalTargetInfo(IntVec3.Invalid, null);

		public string Label
		{
			get
			{
				if (thingInt != null)
				{
					return thingInt.LabelShort;
				}
				if (worldObjectInt != null)
				{
					return worldObjectInt.LabelShort;
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

		public int Tile
		{
			get
			{
				if (worldObjectInt != null)
				{
					return worldObjectInt.Tile;
				}
				if (tileInt >= 0)
				{
					return tileInt;
				}
				if (thingInt != null && thingInt.Tile >= 0)
				{
					return thingInt.Tile;
				}
				if (cellInt.IsValid && mapInt != null)
				{
					return mapInt.Tile;
				}
				return -1;
			}
		}

		public GlobalTargetInfo(Thing thing)
		{
			thingInt = thing;
			cellInt = IntVec3.Invalid;
			mapInt = null;
			worldObjectInt = null;
			tileInt = -1;
		}

		public GlobalTargetInfo(IntVec3 cell, Map map, bool allowNullMap = false)
		{
			if (!allowNullMap && cell.IsValid && map == null)
			{
				Log.Warning("Constructed GlobalTargetInfo with cell=" + cell + " and a null map.");
			}
			thingInt = null;
			cellInt = cell;
			mapInt = map;
			worldObjectInt = null;
			tileInt = -1;
		}

		public GlobalTargetInfo(WorldObject worldObject)
		{
			thingInt = null;
			cellInt = IntVec3.Invalid;
			mapInt = null;
			worldObjectInt = worldObject;
			tileInt = -1;
		}

		public GlobalTargetInfo(int tile)
		{
			thingInt = null;
			cellInt = IntVec3.Invalid;
			mapInt = null;
			worldObjectInt = null;
			tileInt = tile;
		}

		public static implicit operator GlobalTargetInfo(TargetInfo target)
		{
			if (target.HasThing)
			{
				return new GlobalTargetInfo(target.Thing);
			}
			return new GlobalTargetInfo(target.Cell, target.Map);
		}

		public static implicit operator GlobalTargetInfo(Thing t)
		{
			return new GlobalTargetInfo(t);
		}

		public static implicit operator GlobalTargetInfo(WorldObject o)
		{
			return new GlobalTargetInfo(o);
		}

		public static explicit operator LocalTargetInfo(GlobalTargetInfo targ)
		{
			if (targ.worldObjectInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to LocalTargetInfo but it had WorldObject " + targ.worldObjectInt, 134566);
				return LocalTargetInfo.Invalid;
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to LocalTargetInfo but it had tile " + targ.tileInt, 7833122);
				return LocalTargetInfo.Invalid;
			}
			if (!targ.IsValid)
			{
				return LocalTargetInfo.Invalid;
			}
			if (targ.thingInt != null)
			{
				return new LocalTargetInfo(targ.thingInt);
			}
			return new LocalTargetInfo(targ.cellInt);
		}

		public static explicit operator TargetInfo(GlobalTargetInfo targ)
		{
			if (targ.worldObjectInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to TargetInfo but it had WorldObject " + targ.worldObjectInt, 134566);
				return TargetInfo.Invalid;
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to TargetInfo but it had tile " + targ.tileInt, 7833122);
				return TargetInfo.Invalid;
			}
			if (!targ.IsValid)
			{
				return TargetInfo.Invalid;
			}
			if (targ.thingInt != null)
			{
				return new TargetInfo(targ.thingInt);
			}
			return new TargetInfo(targ.cellInt, targ.mapInt);
		}

		public static explicit operator IntVec3(GlobalTargetInfo targ)
		{
			if (targ.thingInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to IntVec3 but it had Thing " + targ.thingInt, 6324165);
			}
			if (targ.worldObjectInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to IntVec3 but it had WorldObject " + targ.worldObjectInt, 134566);
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to IntVec3 but it had tile " + targ.tileInt, 7833122);
			}
			return targ.Cell;
		}

		public static explicit operator Thing(GlobalTargetInfo targ)
		{
			if (targ.cellInt.IsValid)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to Thing but it had cell " + targ.cellInt, 631672);
			}
			if (targ.worldObjectInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to Thing but it had WorldObject " + targ.worldObjectInt, 134566);
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to Thing but it had tile " + targ.tileInt, 7833122);
			}
			return targ.thingInt;
		}

		public static explicit operator WorldObject(GlobalTargetInfo targ)
		{
			if (targ.thingInt != null)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to WorldObject but it had Thing " + targ.thingInt, 6324165);
			}
			if (targ.cellInt.IsValid)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to WorldObject but it had cell " + targ.cellInt, 631672);
			}
			if (targ.tileInt >= 0)
			{
				Log.ErrorOnce("Casted GlobalTargetInfo to WorldObject but it had tile " + targ.tileInt, 7833122);
			}
			return targ.worldObjectInt;
		}

		public static bool operator ==(GlobalTargetInfo a, GlobalTargetInfo b)
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
			if (a.WorldObject != null || b.WorldObject != null)
			{
				return a.WorldObject == b.WorldObject;
			}
			if (a.tileInt >= 0 || b.tileInt >= 0)
			{
				return a.tileInt == b.tileInt;
			}
			return true;
		}

		public static bool operator !=(GlobalTargetInfo a, GlobalTargetInfo b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is GlobalTargetInfo))
			{
				return false;
			}
			return Equals((GlobalTargetInfo)obj);
		}

		public bool Equals(GlobalTargetInfo other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			if (thingInt != null)
			{
				return thingInt.GetHashCode();
			}
			if (cellInt.IsValid)
			{
				return Gen.HashCombine(cellInt.GetHashCode(), mapInt);
			}
			if (worldObjectInt != null)
			{
				return worldObjectInt.GetHashCode();
			}
			if (tileInt >= 0)
			{
				return tileInt;
			}
			return -1;
		}

		public override string ToString()
		{
			if (thingInt != null)
			{
				return thingInt.GetUniqueLoadID();
			}
			if (cellInt.IsValid)
			{
				return cellInt.ToString() + ", " + ((mapInt != null) ? mapInt.GetUniqueLoadID() : "null");
			}
			if (worldObjectInt != null)
			{
				return "@" + worldObjectInt.GetUniqueLoadID();
			}
			if (tileInt >= 0)
			{
				return tileInt.ToString();
			}
			return "null";
		}
	}
}
