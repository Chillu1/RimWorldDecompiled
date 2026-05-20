using System;
using Verse;

namespace RimWorld;

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
			float num = radius;
			return num.Equals(other.radius);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is CellWithRadius other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = cell.GetHashCode() * 397;
		float num2 = radius;
		return num ^ num2.GetHashCode();
	}
}
