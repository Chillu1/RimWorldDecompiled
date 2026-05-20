using UnityEngine;

namespace Verse;

public class PawnFlyerWorker
{
	public PawnFlyerProperties properties;

	public PawnFlyerWorker(PawnFlyerProperties properties)
	{
		this.properties = properties;
	}

	public virtual float AdjustedProgress(float t)
	{
		AnimationCurve progressCurve = properties.ProgressCurve;
		if (progressCurve == null || progressCurve.length == 0)
		{
			return t;
		}
		return progressCurve.Evaluate(t);
	}

	public virtual float GetHeight(float t)
	{
		return GenMath.InverseParabola(t);
	}
}
