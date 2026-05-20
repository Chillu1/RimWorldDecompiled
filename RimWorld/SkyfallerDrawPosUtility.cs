using UnityEngine;
using Verse;

namespace RimWorld;

public static class SkyfallerDrawPosUtility
{
	public static Vector3 DrawPos_Accelerate(Vector3 center, int ticksToImpact, float angle, float speed, bool flipHorizontal = false, CompSkyfallerRandomizeDirection offsetComp = null)
	{
		ticksToImpact = Mathf.Max(ticksToImpact, 0);
		float dist = Mathf.Pow(ticksToImpact, 0.95f) * 1.7f * speed;
		return PosAtDist(center, dist, angle, flipHorizontal, offsetComp);
	}

	public static Vector3 DrawPos_ConstantSpeed(Vector3 center, int ticksToImpact, float angle, float speed, bool flipHorizontal = false, CompSkyfallerRandomizeDirection offsetComp = null)
	{
		ticksToImpact = Mathf.Max(ticksToImpact, 0);
		float dist = (float)ticksToImpact * speed;
		return PosAtDist(center, dist, angle, flipHorizontal, offsetComp);
	}

	public static Vector3 DrawPos_Decelerate(Vector3 center, int ticksToImpact, float angle, float speed, bool flipHorizontal = false, CompSkyfallerRandomizeDirection offsetComp = null)
	{
		ticksToImpact = Mathf.Max(ticksToImpact, 0);
		float dist = (float)(ticksToImpact * ticksToImpact) * 0.00721f * speed;
		return PosAtDist(center, dist, angle, flipHorizontal, offsetComp);
	}

	private static Vector3 PosAtDist(Vector3 center, float dist, float angle, bool flipHorizontal = false, CompSkyfallerRandomizeDirection offsetComp = null)
	{
		Vector3 vector = Vector3Utility.FromAngleFlat(angle - 90f) * dist + (offsetComp?.Offset ?? Vector3.zero);
		if (flipHorizontal)
		{
			vector.x *= -1f;
		}
		return vector + center;
	}
}
