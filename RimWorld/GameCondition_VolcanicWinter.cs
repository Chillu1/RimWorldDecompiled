using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_VolcanicWinter : GameCondition
	{
		private float MaxTempOffset = -7f;

		private const float AnimalDensityImpact = 0.5f;

		private const float SkyGlow = 0.55f;

		private const float MaxSkyLerpFactor = 0.3f;

		private SkyColorSet VolcanicWinterColors = new SkyColorSet(new ColorInt(0, 0, 0).ToColor, Color.white, new Color(0.6f, 0.6f, 0.6f), 0.65f);

		public override int TransitionTicks => 50000;

		public override float SkyTargetLerpFactor(Map map)
		{
			return GameConditionUtility.LerpInOutValue(this, TransitionTicks, 0.3f);
		}

		public override SkyTarget? SkyTarget(Map map)
		{
			return new SkyTarget(0.55f, VolcanicWinterColors, 1f, 1f);
		}

		public override float TemperatureOffset()
		{
			return GameConditionUtility.LerpInOutValue(this, TransitionTicks, MaxTempOffset);
		}

		public override float AnimalDensityFactor(Map map)
		{
			return 1f - GameConditionUtility.LerpInOutValue(this, TransitionTicks, 0.5f);
		}

		public override bool AllowEnjoyableOutsideNow(Map map)
		{
			return false;
		}
	}
}
