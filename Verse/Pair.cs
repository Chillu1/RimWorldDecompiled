using System;
using System.Collections.Generic;

namespace Verse
{
	public struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
	{
		private T1 first;

		private T2 second;

		public T1 First => first;

		public T2 Second => second;

		public Pair(T1 first, T2 second)
		{
			this.first = first;
			this.second = second;
		}

		public override string ToString()
		{
			return "(" + First.ToString() + ", " + Second.ToString() + ")";
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine(Gen.HashCombine(0, first), second);
		}

		public override bool Equals(object other)
		{
			if (!(other is Pair<T1, T2>))
			{
				return false;
			}
			return Equals((Pair<T1, T2>)other);
		}

		public bool Equals(Pair<T1, T2> other)
		{
			if (EqualityComparer<T1>.Default.Equals(first, other.first))
			{
				return EqualityComparer<T2>.Default.Equals(second, other.second);
			}
			return false;
		}

		public static bool operator ==(Pair<T1, T2> lhs, Pair<T1, T2> rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Pair<T1, T2> lhs, Pair<T1, T2> rhs)
		{
			return !(lhs == rhs);
		}
	}
}
