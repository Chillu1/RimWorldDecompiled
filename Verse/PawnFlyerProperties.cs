using System;
using UnityEngine;

namespace Verse;

public class PawnFlyerProperties
{
	private Type workerClass = typeof(PawnFlyerWorker);

	private SimpleCurve progressCurve;

	[NoTranslate]
	private string shadowTexPath = "Things/Skyfaller/SkyfallerShadowCircle";

	public float flightDurationMin = 0.5f;

	public float flightSpeed = 12f;

	public float heightFactor = 2f;

	public IntRange stunDurationTicksRange = IntRange.Zero;

	private PawnFlyerWorker workerInt;

	private AnimationCurve progressCurveInt;

	private Material cachedShadowMaterial;

	public PawnFlyerWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (PawnFlyerWorker)Activator.CreateInstance(workerClass, this);
			}
			return workerInt;
		}
	}

	public AnimationCurve ProgressCurve
	{
		get
		{
			if (progressCurveInt == null && progressCurve != null)
			{
				progressCurveInt = progressCurve.ToAnimationCurve();
			}
			return progressCurveInt;
		}
	}

	public Material ShadowMaterial
	{
		get
		{
			if (cachedShadowMaterial == null && !shadowTexPath.NullOrEmpty())
			{
				cachedShadowMaterial = MaterialPool.MatFrom(shadowTexPath, ShaderDatabase.Transparent);
			}
			return cachedShadowMaterial;
		}
	}
}
