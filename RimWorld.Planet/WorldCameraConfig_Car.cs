using UnityEngine;

namespace RimWorld.Planet
{
	public class WorldCameraConfig_Car : WorldCameraConfig
	{
		private float targetAngle;

		private float angle;

		private float speed;

		private const float SpeedChangeSpeed = 1.5f;

		private const float AngleChangeSpeed = 0.72f;

		public WorldCameraConfig_Car()
		{
			dollyRateKeys = 0f;
			dollyRateScreenEdge = 0f;
			camRotationDecayFactor = 1f;
			rotationSpeedScale = 0.15f;
		}

		public override void ConfigFixedUpdate_60(ref Vector2 rotationVelocity)
		{
			base.ConfigFixedUpdate_60(ref rotationVelocity);
			float num = 0.0166666675f;
			if (KeyBindingDefOf.MapDolly_Left.IsDown)
			{
				targetAngle += 0.72f * num;
			}
			if (KeyBindingDefOf.MapDolly_Right.IsDown)
			{
				targetAngle -= 0.72f * num;
			}
			if (KeyBindingDefOf.MapDolly_Up.IsDown)
			{
				speed += 1.5f * num;
			}
			if (KeyBindingDefOf.MapDolly_Down.IsDown)
			{
				speed -= 1.5f * num;
				if (speed < 0f)
				{
					speed = 0f;
				}
			}
			angle = Mathf.Lerp(angle, targetAngle, 0.02f);
			rotationVelocity.x = Mathf.Cos(angle) * speed;
			rotationVelocity.y = Mathf.Sin(angle) * speed;
		}
	}
}
