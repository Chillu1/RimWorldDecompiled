using UnityEngine;

namespace RimWorld
{
	internal class AlertBounce
	{
		private float position;

		private float velocity;

		private float lastTime = Time.time;

		private bool idle;

		private const float StartPosition = 300f;

		private const float StartVelocity = -200f;

		private const float Acceleration = 1200f;

		private const float DampingRatio = 3f;

		private const float DampingConstant = 1f;

		private const float MaxDelta = 0.05f;

		public void DoAlertStartEffect()
		{
			position = 300f;
			velocity = -200f;
			lastTime = Time.time;
			idle = false;
		}

		public float CalculateHorizontalOffset()
		{
			if (idle)
			{
				return position;
			}
			float num = Mathf.Min(Time.time - lastTime, 0.05f);
			lastTime = Time.time;
			velocity -= 1200f * num;
			position += velocity * num;
			if (position < 0f)
			{
				position = 0f;
				velocity = Mathf.Max((0f - velocity) / 3f - 1f, 0f);
			}
			if (Mathf.Abs(velocity) < 0.0001f && position < 1f)
			{
				velocity = 0f;
				position = 0f;
				idle = true;
			}
			return position;
		}
	}
}
