using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public static class GenGeo
{
	public static float AngleDifferenceBetween(float A, float B)
	{
		float num = A + 360f;
		float num2 = B + 360f;
		float num3 = 9999f;
		float num4 = 0f;
		num4 = A - B;
		if (num4 < 0f)
		{
			num4 *= -1f;
		}
		if (num4 < num3)
		{
			num3 = num4;
		}
		num4 = num - B;
		if (num4 < 0f)
		{
			num4 *= -1f;
		}
		if (num4 < num3)
		{
			num3 = num4;
		}
		num4 = A - num2;
		if (num4 < 0f)
		{
			num4 *= -1f;
		}
		if (num4 < num3)
		{
			num3 = num4;
		}
		return num3;
	}

	public static float MagnitudeHorizontal(this Vector3 v)
	{
		return (float)Math.Sqrt(v.x * v.x + v.z * v.z);
	}

	public static float MagnitudeHorizontalSquared(this Vector3 v)
	{
		return v.x * v.x + v.z * v.z;
	}

	public static bool LinesIntersect(Vector3 line1V1, Vector3 line1V2, Vector3 line2V1, Vector3 line2V2)
	{
		float num = line1V2.z - line1V1.z;
		float num2 = line1V1.x - line1V2.x;
		float num3 = num * line1V1.x + num2 * line1V1.z;
		float num4 = line2V2.z - line2V1.z;
		float num5 = line2V1.x - line2V2.x;
		float num6 = num4 * line2V1.x + num5 * line2V1.z;
		float num7 = num * num5 - num4 * num2;
		if (num7 == 0f)
		{
			return false;
		}
		float num8 = (num5 * num3 - num2 * num6) / num7;
		float num9 = (num * num6 - num4 * num3) / num7;
		if ((num8 > line1V1.x && num8 > line1V2.x) || (num8 > line2V1.x && num8 > line2V2.x) || (num8 < line1V1.x && num8 < line1V2.x) || (num8 < line2V1.x && num8 < line2V2.x) || (num9 > line1V1.z && num9 > line1V2.z) || (num9 > line2V1.z && num9 > line2V2.z) || (num9 < line1V1.z && num9 < line1V2.z) || (num9 < line2V1.z && num9 < line2V2.z))
		{
			return false;
		}
		return true;
	}

	public static bool IsQuadSelfIntersecting(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
	{
		if (!LinesIntersect(p1, p2, p3, p4) && !LinesIntersect(p1, p2, p4, p3) && !LinesIntersect(p2, p3, p1, p4))
		{
			return LinesIntersect(p3, p4, p1, p2);
		}
		return true;
	}

	public static bool IsQuadSelfIntersecting(IntVec3 p1, IntVec3 p2, IntVec3 p3, IntVec3 p4)
	{
		return IsQuadSelfIntersecting(p1.ToVector3(), p2.ToVector3(), p3.ToVector3(), p4.ToVector3());
	}

	public static bool LineRectIntersection(Vector2 pointOnLine, float slope, Vector2 rectA, Vector2 rectB, ref List<Vector2> intersections)
	{
		if (intersections == null)
		{
			intersections = new List<Vector2>();
		}
		else
		{
			intersections.Clear();
		}
		float xMin = Mathf.Min(rectA.x, rectB.x);
		float xMax = Mathf.Max(rectA.x, rectB.x);
		float yMin = Mathf.Min(rectA.y, rectB.y);
		float yMax = Mathf.Max(rectA.y, rectB.y);
		float y = slope * (xMin - pointOnLine.x) + pointOnLine.y;
		Vector2 vector = new Vector2(xMin, y);
		if (IsPointInRect(vector))
		{
			intersections.Add(vector);
		}
		float y2 = slope * (xMax - pointOnLine.x) + pointOnLine.y;
		Vector2 vector2 = new Vector2(xMax, y2);
		if (IsPointInRect(vector2))
		{
			intersections.Add(vector2);
		}
		if (slope != 0f)
		{
			float x = (yMin - pointOnLine.y) / slope + pointOnLine.x;
			Vector2 vector3 = new Vector2(x, yMin);
			if (IsPointInRect(vector3))
			{
				intersections.Add(vector3);
			}
			float x2 = (yMax - pointOnLine.y) / slope + pointOnLine.x;
			Vector2 vector4 = new Vector2(x2, yMax);
			if (IsPointInRect(vector4))
			{
				intersections.Add(vector4);
			}
		}
		return intersections.Count > 0;
		bool IsPointInRect(Vector2 point)
		{
			if (point.x >= xMin && point.x <= xMax && point.y >= yMin)
			{
				return point.y <= yMax;
			}
			return false;
		}
	}

	public static bool IntersectLineCircle(Vector2 center, float radius, Vector2 lineA, Vector2 lineB)
	{
		Vector2 lhs = center - lineA;
		Vector2 vector = lineB - lineA;
		float num = Vector2.Dot(vector, vector);
		float num2 = Vector2.Dot(lhs, vector) / num;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		else if (num2 > 1f)
		{
			num2 = 1f;
		}
		Vector2 vector2 = vector * num2 + lineA - center;
		return Vector2.Dot(vector2, vector2) <= radius * radius;
	}

	public static bool IntersectLineCircleOutline(Vector2 center, float radius, Vector2 lineA, Vector2 lineB)
	{
		bool num = (lineA - center).sqrMagnitude <= radius * radius;
		bool flag = (lineB - center).sqrMagnitude <= radius * radius;
		if (num && flag)
		{
			return false;
		}
		return IntersectLineCircle(center, radius, lineA, lineB);
	}

	public static Vector3 RegularPolygonVertexPositionVec3(int polygonVertices, int vertexIndex)
	{
		Vector2 vector = RegularPolygonVertexPosition(polygonVertices, vertexIndex);
		return new Vector3(vector.x, 0f, vector.y);
	}

	public static Vector2 RegularPolygonVertexPosition(int polygonVertices, int vertexIndex, float angleOffset = 0f)
	{
		if (vertexIndex < 0 || vertexIndex >= polygonVertices)
		{
			Log.Warning("Vertex index out of bounds. polygonVertices=" + polygonVertices + " vertexIndex=" + vertexIndex);
			return Vector2.zero;
		}
		if (polygonVertices == 1)
		{
			return Vector2.zero;
		}
		return CalculatePolygonVertexPosition(polygonVertices, vertexIndex, angleOffset);
	}

	private static Vector2 CalculatePolygonVertexPosition(int polygonVertices, int vertexIndex, float angleOffset = 0f)
	{
		float num = MathF.PI * 2f / (float)polygonVertices * (float)vertexIndex;
		num += MathF.PI;
		num += MathF.PI / 180f * angleOffset;
		return new Vector3(Mathf.Cos(num), Mathf.Sin(num));
	}

	public static Vector2 InverseQuadBilinear(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		float num = (p0 - p).Cross(p0 - p2);
		float num2 = ((p0 - p).Cross(p1 - p3) + (p1 - p).Cross(p0 - p2)) / 2f;
		float num3 = (p1 - p).Cross(p1 - p3);
		float num4 = num2 * num2 - num * num3;
		if (num4 < 0f)
		{
			return new Vector2(-1f, -1f);
		}
		num4 = Mathf.Sqrt(num4);
		float num5;
		if (Mathf.Abs(num - 2f * num2 + num3) < 0.0001f)
		{
			num5 = num / (num - num3);
		}
		else
		{
			float num6 = (num - num2 + num4) / (num - 2f * num2 + num3);
			float num7 = (num - num2 - num4) / (num - 2f * num2 + num3);
			num5 = ((!(Mathf.Abs(num6 - 0.5f) < Mathf.Abs(num7 - 0.5f))) ? num7 : num6);
		}
		float num8 = (1f - num5) * (p0.x - p2.x) + num5 * (p1.x - p3.x);
		float num9 = (1f - num5) * (p0.y - p2.y) + num5 * (p1.y - p3.y);
		if (Mathf.Abs(num8) < Mathf.Abs(num9))
		{
			return new Vector2(num5, ((1f - num5) * (p0.y - p.y) + num5 * (p1.y - p.y)) / num9);
		}
		return new Vector2(num5, ((1f - num5) * (p0.x - p.x) + num5 * (p1.x - p.x)) / num8);
	}

	public static int GetAdjacencyScore(this CellRect rect, CellRect other)
	{
		if (rect.Overlaps(other))
		{
			return 0;
		}
		if (rect.maxZ == other.minZ - 1 && rect.minX < other.maxX && rect.maxX > other.minX)
		{
			int num = Mathf.Max(rect.minX, other.minX);
			return Mathf.Min(rect.maxX, other.maxX) - num;
		}
		if (rect.minZ == other.maxZ + 1 && rect.minX < other.maxX && rect.maxX > other.minX)
		{
			int num2 = Mathf.Max(rect.minX, other.minX);
			return Mathf.Min(rect.maxX, other.maxX) - num2;
		}
		if (rect.minX == other.maxX + 1 && rect.minZ < other.maxZ && rect.maxZ > other.minZ)
		{
			int num3 = Mathf.Max(rect.minZ, other.minZ);
			return Mathf.Min(rect.maxZ, other.maxZ) - num3;
		}
		if (rect.maxX == other.minX - 1 && rect.minZ < other.maxZ && rect.maxZ > other.minZ)
		{
			int num4 = Mathf.Max(rect.minZ, other.minZ);
			return Mathf.Min(rect.maxZ, other.maxZ) - num4;
		}
		return 0;
	}

	public static CellRect ExpandToFit(this CellRect rect, IntVec3 position)
	{
		if (rect.Contains(position))
		{
			return rect;
		}
		rect.minX = Mathf.Min(rect.minX, position.x);
		rect.minZ = Mathf.Min(rect.minZ, position.z);
		rect.maxX = Mathf.Max(rect.maxX, position.x);
		rect.maxZ = Mathf.Max(rect.maxZ, position.z);
		return rect;
	}
}
