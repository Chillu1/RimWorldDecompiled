using System;
using System.Globalization;
using UnityEngine;

namespace Verse;

public struct IntVec2 : IEquatable<IntVec2>
{
	public int x;

	public int z;

	public bool IsInvalid => x < -500;

	public bool IsValid => x >= -500;

	public static IntVec2 Zero => new IntVec2(0, 0);

	public static IntVec2 One => new IntVec2(1, 1);

	public static IntVec2 Two => new IntVec2(2, 2);

	public static IntVec2 North => new IntVec2(0, 1);

	public static IntVec2 East => new IntVec2(1, 0);

	public static IntVec2 South => new IntVec2(0, -1);

	public static IntVec2 West => new IntVec2(-1, 0);

	public float Magnitude => Mathf.Sqrt(x * x + z * z);

	public int MagnitudeManhattan => Mathf.Abs(x) + Mathf.Abs(z);

	public int Area => Mathf.Abs(x) * Mathf.Abs(z);

	public static IntVec2 Invalid => new IntVec2(-1000, -1000);

	public IntVec3 ToIntVec3 => new IntVec3(x, 0, z);

	public IntVec2(int newX, int newZ)
	{
		x = newX;
		z = newZ;
	}

	public IntVec2(Vector2 v2)
	{
		x = (int)v2.x;
		z = (int)v2.y;
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, z);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, 0f, z);
	}

	public IntVec2 Rotated()
	{
		return new IntVec2(z, x);
	}

	public override string ToString()
	{
		return "(" + x + ", " + z + ")";
	}

	public string ToStringCross()
	{
		return x + " x " + z;
	}

	public static IntVec2 FromString(string str)
	{
		str = str.TrimStart('(');
		str = str.TrimEnd(')');
		string[] array = str.Split(',');
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		int newX = Convert.ToInt32(array[0], invariantCulture);
		int newZ = Convert.ToInt32(array[1], invariantCulture);
		return new IntVec2(newX, newZ);
	}

	public Vector2 ToVector2Shifted()
	{
		return new Vector2((float)x + 0.5f, (float)z + 0.5f);
	}

	public static IntVec2 operator +(IntVec2 a, IntVec2 b)
	{
		return new IntVec2(a.x + b.x, a.z + b.z);
	}

	public static IntVec2 operator -(IntVec2 a, IntVec2 b)
	{
		return new IntVec2(a.x - b.x, a.z - b.z);
	}

	public static IntVec2 operator *(IntVec2 a, int b)
	{
		return new IntVec2(a.x * b, a.z * b);
	}

	public static IntVec2 operator *(IntVec2 a, float b)
	{
		return new IntVec2(Mathf.RoundToInt((float)a.x * b), Mathf.RoundToInt((float)a.z * b));
	}

	public static IntVec2 operator /(IntVec2 a, int b)
	{
		return new IntVec2(a.x / b, a.z / b);
	}

	public static bool operator ==(IntVec2 a, IntVec2 b)
	{
		if (a.x == b.x && a.z == b.z)
		{
			return true;
		}
		return false;
	}

	public static bool operator !=(IntVec2 a, IntVec2 b)
	{
		if (a.x != b.x || a.z != b.z)
		{
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is IntVec2))
		{
			return false;
		}
		return Equals((IntVec2)obj);
	}

	public bool Equals(IntVec2 other)
	{
		if (x == other.x)
		{
			return z == other.z;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineInt(x, z);
	}
}
