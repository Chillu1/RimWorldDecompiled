using System;

namespace Verse
{
	public struct ThingCount : IEquatable<ThingCount>, IExposable
	{
		private Thing thing;

		private int count;

		public Thing Thing => thing;

		public int Count => count;

		public ThingCount(Thing thing, int count)
		{
			if (count < 0)
			{
				Log.Warning("Tried to set ThingCount stack count to " + count + ". thing=" + thing);
				count = 0;
			}
			if (count > thing.stackCount)
			{
				Log.Warning("Tried to set ThingCount stack count to " + count + ", but thing's stack count is only " + thing.stackCount + ". thing=" + thing);
				count = thing.stackCount;
			}
			this.thing = thing;
			this.count = count;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref thing, "thing");
			Scribe_Values.Look(ref count, "count", 1);
		}

		public ThingCount WithCount(int newCount)
		{
			return new ThingCount(thing, newCount);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ThingCount))
			{
				return false;
			}
			return Equals((ThingCount)obj);
		}

		public bool Equals(ThingCount other)
		{
			return this == other;
		}

		public static bool operator ==(ThingCount a, ThingCount b)
		{
			if (a.thing == b.thing)
			{
				return a.count == b.count;
			}
			return false;
		}

		public static bool operator !=(ThingCount a, ThingCount b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine(count, thing);
		}

		public override string ToString()
		{
			return "(" + count + "x " + ((thing != null) ? thing.LabelShort : "null") + ")";
		}

		public static implicit operator ThingCount(ThingCountClass t)
		{
			if (t == null)
			{
				return new ThingCount(null, 0);
			}
			return new ThingCount(t.thing, t.Count);
		}
	}
}
