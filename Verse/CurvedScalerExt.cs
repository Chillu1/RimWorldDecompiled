using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public static class CurvedScalerExt
{
	public static Vector3 ScaleAtTime(this List<CurvedScaler> scalers, float ageSecs)
	{
		Vector3 one = Vector3.one;
		foreach (CurvedScaler scaler in scalers)
		{
			Rand.PushState(scaler.GetHashCode());
			float randomInRange = scaler.scaleTime.RandomInRange;
			float randomInRange2 = scaler.scaleAmt.RandomInRange;
			float x = Mathf.Clamp01(ageSecs / randomInRange);
			one += randomInRange2 * scaler.curve.Evaluate(x) * scaler.axisMask;
			Rand.PopState();
		}
		return one;
	}
}
