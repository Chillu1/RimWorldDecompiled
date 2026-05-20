using UnityEngine;

namespace RimWorld;

public static class Bezier
{
	public static Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
	{
		Vector2 a2 = Vector2.Lerp(a, b, t);
		Vector2 b2 = Vector2.Lerp(b, c, t);
		return Vector2.Lerp(a2, b2, t);
	}

	public static Vector2 EvaluateCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
	{
		Vector2 a2 = EvaluateQuadratic(a, b, c, t);
		Vector2 b2 = EvaluateQuadratic(b, c, d, t);
		return Vector2.Lerp(a2, b2, t);
	}
}
