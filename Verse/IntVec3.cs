using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Verse;

public struct IntVec3 : IEquatable<IntVec3>
{
	public int x;

	public int y;

	public int z;

	public static readonly IntVec3 Zero = new IntVec3(0, 0, 0);

	public static readonly IntVec3 North = new IntVec3(0, 0, 1);

	public static readonly IntVec3 East = new IntVec3(1, 0, 0);

	public static readonly IntVec3 South = new IntVec3(0, 0, -1);

	public static readonly IntVec3 West = new IntVec3(-1, 0, 0);

	public static readonly IntVec3 NorthWest = new IntVec3(-1, 0, 1);

	public static readonly IntVec3 NorthEast = new IntVec3(1, 0, 1);

	public static readonly IntVec3 SouthWest = new IntVec3(-1, 0, -1);

	public static readonly IntVec3 SouthEast = new IntVec3(1, 0, -1);

	public static readonly IntVec3 Invalid = new IntVec3(-1000, -1000, -1000);

	public IntVec2 ToIntVec2 => new IntVec2(x, z);

	public bool IsValid
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return y >= 0;
		}
	}

	public int LengthHorizontalSquared => x * x + z * z;

	public float LengthHorizontal => GenMath.Sqrt(x * x + z * z);

	public int LengthManhattan => ((x >= 0) ? x : (-x)) + ((z >= 0) ? z : (-z));

	public float Magnitude => Mathf.Sqrt(x * x + z * z);

	public float SqrMagnitude => x * x + z * z;

	public bool IsCardinal
	{
		get
		{
			if (x != 0)
			{
				return z == 0;
			}
			return true;
		}
	}

	public float AngleFlat
	{
		get
		{
			if (x == 0 && z == 0)
			{
				return 0f;
			}
			return Quaternion.LookRotation(ToVector3()).eulerAngles.y;
		}
	}

	public static implicit operator int3(IntVec3 value)
	{
		return new int3(value.x, value.y, value.z);
	}

	public static implicit operator float3(IntVec3 value)
	{
		return new float3(value.x, value.y, value.z);
	}

	public static implicit operator IntVec3(int3 value)
	{
		return new IntVec3(value.x, value.y, value.z);
	}

	public static implicit operator IntVec3(float3 value)
	{
		return new IntVec3(Mathf.FloorToInt(value.x), Mathf.FloorToInt(value.y), Mathf.FloorToInt(value.z));
	}

	public IntVec3(int newX, int newY, int newZ)
	{
		x = newX;
		y = newY;
		z = newZ;
	}

	public IntVec3(Vector3 v)
	{
		x = (int)v.x;
		y = 0;
		z = (int)v.z;
	}

	public IntVec3(Vector2 v)
	{
		x = (int)v.x;
		y = 0;
		z = (int)v.y;
	}

	public static IntVec3 operator -(IntVec3 a)
	{
		return new IntVec3(-a.x, -a.y, -a.z);
	}

	public static IntVec3 FromString(string str)
	{
		str = str.TrimStart('(');
		str = str.TrimEnd(')');
		string[] array = str.Split(',');
		try
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			int newX = Convert.ToInt32(array[0], invariantCulture);
			int newY = Convert.ToInt32(array[1], invariantCulture);
			int newZ = Convert.ToInt32(array[2], invariantCulture);
			return new IntVec3(newX, newY, newZ);
		}
		catch (Exception ex)
		{
			Log.Warning(str + " is not a valid IntVec3 format. Exception: " + ex);
			return Invalid;
		}
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, z);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, y, z);
	}

	public Vector3 ToVector3Shifted()
	{
		return new Vector3((float)x + 0.5f, y, (float)z + 0.5f);
	}

	public Vector3 ToVector3ShiftedWithAltitude(AltitudeLayer AltLayer)
	{
		return ToVector3ShiftedWithAltitude(AltLayer.AltitudeFor());
	}

	public Vector3 ToVector3ShiftedWithAltitude(float AddedAltitude)
	{
		return new Vector3((float)x + 0.5f, (float)y + AddedAltitude, (float)z + 0.5f);
	}

	public bool InHorDistOf(IntVec3 otherLoc, float maxDist)
	{
		float num = x - otherLoc.x;
		float num2 = z - otherLoc.z;
		return num * num + num2 * num2 <= maxDist * maxDist;
	}

	public static IntVec3 FromVector3(Vector3 v)
	{
		return FromVector3(v, 0);
	}

	public static IntVec3 FromVector3(Vector3 v, int newY)
	{
		return new IntVec3((int)v.x, newY, (int)v.z);
	}

	public static IntVec3 FromPolar(float angle, float distance)
	{
		float num = Mathf.Cos(angle * (MathF.PI / 180f)) * distance;
		float num2 = Mathf.Sin(angle * (MathF.PI / 180f)) * distance;
		return new IntVec3((int)num, 0, (int)num2);
	}

	public Vector2 ToUIPosition()
	{
		return ToVector3Shifted().MapToUIPosition();
	}

	public Rect ToUIRect()
	{
		Vector2 vector = ToVector3().MapToUIPosition();
		Vector2 vector2 = (this + NorthEast).ToVector3().MapToUIPosition();
		return new Rect(vector.x, vector2.y, vector2.x - vector.x, vector.y - vector2.y);
	}

	public bool AdjacentToCardinal(IntVec3 other)
	{
		if (!IsValid)
		{
			return false;
		}
		if (other.z == z && (other.x == x + 1 || other.x == x - 1))
		{
			return true;
		}
		if (other.x == x && (other.z == z + 1 || other.z == z - 1))
		{
			return true;
		}
		return false;
	}

	public bool CardinalTo(IntVec3 other)
	{
		if (!IsValid || !other.IsValid)
		{
			return false;
		}
		if (other.z != z)
		{
			return other.x == x;
		}
		return true;
	}

	public bool AdjacentToDiagonal(IntVec3 other)
	{
		if (!IsValid)
		{
			return false;
		}
		if (Mathf.Abs(x - other.x) == 1)
		{
			return Mathf.Abs(z - other.z) == 1;
		}
		return false;
	}

	public bool AdjacentTo(IntVec3 other)
	{
		if (!AdjacentToDiagonal(other))
		{
			return AdjacentToCardinal(other);
		}
		return true;
	}

	public bool AdjacentToCardinal(District district)
	{
		if (!IsValid)
		{
			return false;
		}
		Map map = district.Map;
		if (this.InBounds(map) && this.GetDistrict(map, RegionType.Set_All) == district)
		{
			return true;
		}
		IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
		for (int i = 0; i < cardinalDirections.Length; i++)
		{
			IntVec3 intVec = this + cardinalDirections[i];
			if (intVec.InBounds(map) && intVec.GetDistrict(map, RegionType.Set_All) == district)
			{
				return true;
			}
		}
		return false;
	}

	public IntVec3 ClampInsideMap(Map map)
	{
		return ClampInsideRect(CellRect.WholeMap(map));
	}

	public IntVec3 ClampMagnitude(float magnitude)
	{
		float lengthHorizontal = LengthHorizontal;
		if (lengthHorizontal <= magnitude)
		{
			return this;
		}
		float num = (float)x / lengthHorizontal;
		float num2 = (float)z / lengthHorizontal;
		x = Mathf.RoundToInt(num * magnitude);
		z = Mathf.RoundToInt(num2 * magnitude);
		return this;
	}

	public IntVec3 ClampInsideRect(CellRect rect)
	{
		x = Mathf.Clamp(x, rect.minX, rect.maxX);
		y = 0;
		z = Mathf.Clamp(z, rect.minZ, rect.maxZ);
		return this;
	}

	public static IntVec3 operator +(IntVec3 a, IntVec3 b)
	{
		return new IntVec3(a.x + b.x, a.y + b.y, a.z + b.z);
	}

	public static IntVec3 operator -(IntVec3 a, IntVec3 b)
	{
		return new IntVec3(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	public static IntVec3 operator *(IntVec3 a, int i)
	{
		return new IntVec3(a.x * i, a.y * i, a.z * i);
	}

	public static IntVec3 operator *(int i, IntVec3 a)
	{
		return new IntVec3(a.x * i, a.y * i, a.z * i);
	}

	public static IntVec3 operator /(IntVec3 a, int i)
	{
		return new IntVec3(a.x / i, a.y / i, a.z / i);
	}

	public static bool operator ==(IntVec3 a, IntVec3 b)
	{
		if (a.x == b.x && a.z == b.z && a.y == b.y)
		{
			return true;
		}
		return false;
	}

	public static bool operator !=(IntVec3 a, IntVec3 b)
	{
		if (a.x != b.x || a.z != b.z || a.y != b.y)
		{
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is IntVec3)
		{
			return Equals((IntVec3)obj);
		}
		return false;
	}

	public bool Equals(IntVec3 other)
	{
		if (x == other.x && z == other.z)
		{
			return y == other.y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Gen.HashCombineInt(Gen.HashCombineInt(Gen.HashCombineInt(0, x), y), z);
	}

	public ulong UniqueHashCode()
	{
		return (ulong)(0L + (long)x + 4096L * (long)z + 16777216L * (long)y);
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ", " + z + ")";
	}
}
