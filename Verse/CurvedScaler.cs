using UnityEngine;

namespace Verse;

public class CurvedScaler
{
	public SimpleCurve curve;

	public Vector3 axisMask = Vector3.one;

	public FloatRange scaleAmt;

	public FloatRange scaleTime;
}
