using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse;

public class SimpleCurveView
{
	public Rect rect;

	private Dictionary<object, float> debugInputValues = new Dictionary<object, float>();

	private const float ResetZoomBuffer = 0.1f;

	private static Rect identityRect = new Rect(0f, 0f, 1f, 1f);

	public IEnumerable<float> DebugInputValues
	{
		get
		{
			if (debugInputValues == null)
			{
				yield break;
			}
			foreach (float value in debugInputValues.Values)
			{
				yield return value;
			}
		}
	}

	public void SetDebugInput(object key, float value)
	{
		debugInputValues[key] = value;
	}

	public void ClearDebugInputFrom(object key)
	{
		if (debugInputValues.ContainsKey(key))
		{
			debugInputValues.Remove(key);
		}
	}

	public void SetViewRectAround(SimpleCurve curve)
	{
		if (curve.PointsCount == 0)
		{
			rect = identityRect;
			return;
		}
		rect.xMin = curve.Points.Select((CurvePoint pt) => pt.Loc.x).Min();
		rect.xMax = curve.Points.Select((CurvePoint pt) => pt.Loc.x).Max();
		rect.yMin = curve.Points.Select((CurvePoint pt) => pt.Loc.y).Min();
		rect.yMax = curve.Points.Select((CurvePoint pt) => pt.Loc.y).Max();
		if (Mathf.Approximately(rect.width, 0f))
		{
			rect.width = rect.xMin * 2f;
		}
		if (Mathf.Approximately(rect.height, 0f))
		{
			rect.height = rect.yMin * 2f;
		}
		if (Mathf.Approximately(rect.width, 0f))
		{
			rect.width = 1f;
		}
		if (Mathf.Approximately(rect.height, 0f))
		{
			rect.height = 1f;
		}
		float width = rect.width;
		float height = rect.height;
		rect.xMin -= width * 0.1f;
		rect.xMax += width * 0.1f;
		rect.yMin -= height * 0.1f;
		rect.yMax += height * 0.1f;
	}
}
