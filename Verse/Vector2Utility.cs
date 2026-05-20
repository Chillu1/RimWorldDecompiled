using System;
using UnityEngine;

namespace Verse;

public static class Vector2Utility
{
	public static Vector2 Rotated(this Vector2 v)
	{
		return new Vector2(v.y, v.x);
	}

	public static Vector2 RotatedBy(this Vector2 v, Rot4 rot)
	{
		return v.RotatedBy(rot.AsAngle);
	}

	public static Vector2 RotatedBy(this Vector2 v, float degrees)
	{
		float num = Mathf.Sin(degrees * (MathF.PI / 180f));
		float num2 = Mathf.Cos(degrees * (MathF.PI / 180f));
		return new Vector2(num2 * v.x - num * v.y, num * v.x + num2 * v.y);
	}

	public static float AngleTo(this Vector2 a, Vector2 b)
	{
		return Mathf.Atan2(0f - (b.y - a.y), b.x - a.x) * 57.29578f;
	}

	public static Vector3 ScaledBy(this Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static Vector2 Moved(this Vector2 v, float angle, float distance)
	{
		return new Vector2(v.x + Mathf.Cos(angle * (MathF.PI / 180f)) * distance, v.y - Mathf.Sin(angle * (MathF.PI / 180f)) * distance);
	}

	public static Vector2 FromAngle(float angle)
	{
		return new Vector2(Mathf.Cos(angle * (MathF.PI / 180f)), 0f - Mathf.Sin(angle * (MathF.PI / 180f)));
	}

	public static float ToAngle(this Vector2 v)
	{
		return Mathf.Atan2(0f - v.y, v.x) * 57.29578f;
	}

	public static Vector3 ToVector3(this Vector2 v)
	{
		return new Vector3(v.x, 0f, v.y);
	}

	public static Vector2 ToVector2(this Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

	public static float Cross(this Vector2 u, Vector2 v)
	{
		return u.x * v.y - u.y * v.x;
	}

	public static float DistanceToRect(this Vector2 u, Rect rect)
	{
		if (rect.Contains(u))
		{
			return 0f;
		}
		if (u.x < rect.xMin && u.y < rect.yMin)
		{
			return Vector2.Distance(u, new Vector2(rect.xMin, rect.yMin));
		}
		if (u.x > rect.xMax && u.y < rect.yMin)
		{
			return Vector2.Distance(u, new Vector2(rect.xMax, rect.yMin));
		}
		if (u.x < rect.xMin && u.y > rect.yMax)
		{
			return Vector2.Distance(u, new Vector2(rect.xMin, rect.yMax));
		}
		if (u.x > rect.xMax && u.y > rect.yMax)
		{
			return Vector2.Distance(u, new Vector2(rect.xMax, rect.yMax));
		}
		if (u.x < rect.xMin)
		{
			return rect.xMin - u.x;
		}
		if (u.x > rect.xMax)
		{
			return u.x - rect.xMax;
		}
		if (u.y < rect.yMin)
		{
			return rect.yMin - u.y;
		}
		return u.y - rect.yMax;
	}

	public static Vector3 MultipliedBy(this Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static Vector2 Cardinalize(this Vector2 vec)
	{
		return vec.Cardinalize(Vector2.zero);
	}

	public static Vector2 Cardinalize(this Vector2 vec, Vector2 bias)
	{
		if (vec.sqrMagnitude == 0f)
		{
			return vec;
		}
		Vector2 normalized = (vec + bias).normalized;
		float num = Mathf.Abs(normalized.x);
		float num2 = Mathf.Abs(normalized.y);
		if (num > num2)
		{
			if (normalized.x > 0f)
			{
				return Vector2.right;
			}
			return Vector2.left;
		}
		if (normalized.y > 0f)
		{
			return Vector2.up;
		}
		return Vector2.down;
	}
}
