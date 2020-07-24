using System;
using Verse;

namespace RimWorld
{
	public struct CellWithRadius : IEquatable<CellWithRadius>
	{
		public readonly IntVec3 cell;

		public readonly float radius;

		public CellWithRadius(IntVec3 c, float r)
		{
			cell = c;
			radius = r;
		}

		public bool Equals(CellWithRadius other)
		{
			if (cell.Equals(other.cell))
			{
				return radius.Equals(other.radius);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			object obj2;
			if ((obj2 = obj) is CellWithRadius)
			{
				CellWithRadius other = (CellWithRadius)obj2;
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			IntVec3 intVec = cell;
			return (intVec.GetHashCode() * 397) ^ radius.GetHashCode();
		}
	}
}
