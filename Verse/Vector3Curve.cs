using LudeonTK;
using UnityEngine;

namespace Verse;

public class Vector3Curve
{
	public ComplexCurve x;

	public ComplexCurve y;

	public ComplexCurve z;

	public Vector3 Evaluate(float time, float nullCurveValue = 0f)
	{
		float num = x?.Evaluate(time) ?? nullCurveValue;
		float num2 = y?.Evaluate(time) ?? nullCurveValue;
		float num3 = z?.Evaluate(time) ?? nullCurveValue;
		return new Vector3(num, num2, num3);
	}
}
