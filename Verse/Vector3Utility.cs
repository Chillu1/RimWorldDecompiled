using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse;

public static class Vector3Utility
{
	public static Vector3 HorizontalVectorFromAngle(float angle)
	{
		return Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
	}

	public static float AngleFlat(this Vector3 v)
	{
		if (v.x == 0f && v.z == 0f)
		{
			return 0f;
		}
		return Quaternion.LookRotation(v).eulerAngles.y;
	}

	public static Vector3 RandomHorizontalOffset(float maxDist)
	{
		float num = Rand.Range(0f, maxDist);
		float y = Rand.Range(0, 360);
		return Quaternion.Euler(new Vector3(0f, y, 0f)) * Vector3.forward * num;
	}

	public static Vector3 Yto0(this Vector3 v3)
	{
		return new Vector3(v3.x, 0f, v3.z);
	}

	public static Vector3 WithYOffset(this Vector3 v3, float offset)
	{
		return new Vector3(v3.x, v3.y + offset, v3.z);
	}

	public static Vector3 WithY(this Vector3 v3, float y)
	{
		return new Vector3(v3.x, y, v3.z);
	}

	public static Vector3 SetToAltitude(this Vector3 v3, AltitudeLayer altitude)
	{
		v3.y = altitude.AltitudeFor();
		return v3;
	}

	public static Vector3 RotatedBy(this Vector3 v3, float angle)
	{
		return Quaternion.AngleAxis(angle, Vector3.up) * v3;
	}

	public static Vector3 RotatedBy(this Vector3 orig, Rot4 rot)
	{
		return rot.AsInt switch
		{
			0 => orig, 
			1 => new Vector3(orig.z, orig.y, 0f - orig.x), 
			2 => new Vector3(0f - orig.x, orig.y, 0f - orig.z), 
			3 => new Vector3(0f - orig.z, orig.y, orig.x), 
			_ => orig, 
		};
	}

	public static float AngleToFlat(this Vector3 a, Vector3 b)
	{
		return new Vector2(a.x, a.z).AngleTo(new Vector2(b.x, b.z));
	}

	public static Vector3 FromAngleFlat(float angle)
	{
		Vector2 vector = Vector2Utility.FromAngle(angle);
		return new Vector3(vector.x, 0f, vector.y);
	}

	public static float ToAngleFlat(this Vector3 v)
	{
		return new Vector2(v.x, v.z).ToAngle();
	}

	public static Vector3 Abs(this Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	public static Vector3 ClosestTo(this IEnumerable<Vector3> pts, Vector3 target)
	{
		float num = float.MaxValue;
		Vector3 result = pts.FirstOrDefault();
		foreach (Vector3 pt in pts)
		{
			float num2 = Vector3.Distance(pt, target);
			if (num2 < num)
			{
				num = num2;
				result = pt;
			}
		}
		return result;
	}
}
