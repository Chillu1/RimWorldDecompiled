using UnityEngine;

namespace RimWorld
{
	public static class GameConditionUtility
	{
		public static float LerpInOutValue(GameCondition gameCondition, float lerpTime, float lerpTarget = 1f)
		{
			if (gameCondition.Permanent)
			{
				return LerpInOutValue(gameCondition.TicksPassed, lerpTime + 1f, lerpTime, lerpTarget);
			}
			return LerpInOutValue(gameCondition.TicksPassed, gameCondition.TicksLeft, lerpTime, lerpTarget);
		}

		public static float LerpInOutValue(float timePassed, float timeLeft, float lerpTime, float lerpTarget = 1f)
		{
			float num = 1f;
			if (timePassed < lerpTime)
			{
				num = timePassed / lerpTime;
			}
			if (timeLeft < lerpTime)
			{
				num = Mathf.Min(num, timeLeft / lerpTime);
			}
			return Mathf.Lerp(0f, lerpTarget, num);
		}
	}
}
