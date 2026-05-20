namespace Verse;

public static class ShootTuning
{
	public const float DistTouch = 3f;

	public const float DistShort = 12f;

	public const float DistMedium = 25f;

	public const float DistLong = 40f;

	public const float MeleeRange = 1.42f;

	public const float HitChanceFactorFromEquipmentMin = 0.01f;

	public const float MinAccuracyFactorFromShooterAndDistance = 0.0201f;

	public const float LayingDownHitChanceFactorMinDistance = 4.5f;

	public const float HitChanceFactorIfLayingDown = 0.5f;

	public const float ExecutionMaxDistance = 3.9f;

	public const float ExecutionAccuracyFactor = 7.5f;

	public const float TargetSizeFactorFromFillPercentFactor = 2.5f;

	public const float TargetSizeFactorMin = 0.5f;

	public const float TargetSizeFactorMax = 2f;

	public const float MinAimOnChance_StandardTarget = 0.0201f;

	public static readonly SimpleSurface MissDistanceFromAimOnChanceCurves = new SimpleSurface
	{
		new SurfaceColumn(0.02f, new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 10f)
		}),
		new SurfaceColumn(0.04f, new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 8f)
		}),
		new SurfaceColumn(0.07f, new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 6f)
		}),
		new SurfaceColumn(0.11f, new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 4f)
		}),
		new SurfaceColumn(0.22f, new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 2f)
		}),
		new SurfaceColumn(1f, new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 1f)
		})
	};

	public const float CanInterceptPawnsChanceOnWildOrForcedMissRadius = 0.5f;

	public const float InterceptDistMin = 5f;

	public const float InterceptDistMax = 12f;

	public const float Intercept_Pawn_HitChancePerBodySize = 0.4f;

	public const float Intercept_Pawn_HitChanceFactor_LayingDown = 0.1f;

	public const float Intercept_Object_HitChancePerFillPercent = 0.15f;

	public const float Intercept_Object_AdjToTarget_HitChancePerFillPercent = 1f;

	public const float Intercept_OpenDoor_HitChance = 0.05f;

	public const float ImpactCell_Pawn_HitChancePerBodySize = 0.5f;

	public const float ImpactCell_Object_HitChancePerFillPercent = 1.5f;

	public const float BodySizeClampMin = 0.1f;

	public const float BodySizeClampMax = 2f;
}
