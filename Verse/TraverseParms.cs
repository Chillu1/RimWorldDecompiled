using System;

namespace Verse
{
	public struct TraverseParms : IEquatable<TraverseParms>
	{
		public Pawn pawn;

		public TraverseMode mode;

		public Danger maxDanger;

		public bool canBash;

		public static TraverseParms For(Pawn pawn, Danger maxDanger = Danger.Deadly, TraverseMode mode = TraverseMode.ByPawn, bool canBash = false)
		{
			if (pawn == null)
			{
				Log.Error("TraverseParms for null pawn.");
				return For(TraverseMode.NoPassClosedDoors, maxDanger, canBash);
			}
			TraverseParms result = default(TraverseParms);
			result.pawn = pawn;
			result.maxDanger = maxDanger;
			result.mode = mode;
			result.canBash = canBash;
			return result;
		}

		public static TraverseParms For(TraverseMode mode, Danger maxDanger = Danger.Deadly, bool canBash = false)
		{
			TraverseParms result = default(TraverseParms);
			result.pawn = null;
			result.mode = mode;
			result.maxDanger = maxDanger;
			result.canBash = canBash;
			return result;
		}

		public void Validate()
		{
			if (mode == TraverseMode.ByPawn && pawn == null)
			{
				Log.Error("Invalid traverse parameters: IfPawnAllowed but traverser = null.");
			}
		}

		public static implicit operator TraverseParms(TraverseMode m)
		{
			if (m == TraverseMode.ByPawn)
			{
				throw new InvalidOperationException("Cannot implicitly convert TraverseMode.ByPawn to RegionTraverseParameters.");
			}
			return For(m);
		}

		public static bool operator ==(TraverseParms a, TraverseParms b)
		{
			if (a.pawn == b.pawn && a.mode == b.mode && a.canBash == b.canBash)
			{
				return a.maxDanger == b.maxDanger;
			}
			return false;
		}

		public static bool operator !=(TraverseParms a, TraverseParms b)
		{
			if (a.pawn == b.pawn && a.mode == b.mode && a.canBash == b.canBash)
			{
				return a.maxDanger != b.maxDanger;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TraverseParms))
			{
				return false;
			}
			return Equals((TraverseParms)obj);
		}

		public bool Equals(TraverseParms other)
		{
			if (other.pawn == pawn && other.mode == mode && other.canBash == canBash)
			{
				return other.maxDanger == maxDanger;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int seed = canBash ? 1 : 0;
			seed = ((pawn == null) ? Gen.HashCombineStruct(seed, mode) : Gen.HashCombine(seed, pawn));
			return Gen.HashCombineStruct(seed, maxDanger);
		}

		public override string ToString()
		{
			string text = canBash ? " canBash" : "";
			if (mode == TraverseMode.ByPawn)
			{
				return "(" + mode + " " + maxDanger + " " + pawn + text + ")";
			}
			return "(" + mode + " " + maxDanger + text + ")";
		}
	}
}
