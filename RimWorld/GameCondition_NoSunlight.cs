using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_NoSunlight : GameCondition
	{
		private SkyColorSet EclipseSkyColors = new SkyColorSet(new Color(0.482f, 0.603f, 0.682f), Color.white, new Color(0.6f, 0.6f, 0.6f), 1f);

		public override int TransitionTicks => 200;

		public override float SkyTargetLerpFactor(Map map)
		{
			return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
		}

		public override SkyTarget? SkyTarget(Map map)
		{
			return new SkyTarget(0f, EclipseSkyColors, 1f, 0f);
		}
	}
}
