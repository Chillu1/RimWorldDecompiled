using RimWorld;
using UnityEngine;

namespace Verse
{
	public class CameraMapConfig_Car : CameraMapConfig
	{
		private float targetAngle;

		private float angle;

		private float speed;

		private const float SpeedChangeSpeed = 1.2f;

		private const float AngleChangeSpeed = 0.72f;

		public CameraMapConfig_Car()
		{
			dollyRateKeys = 0f;
			dollyRateScreenEdge = 0f;
			camSpeedDecayFactor = 1f;
			moveSpeedScale = 1f;
		}

		public override void ConfigFixedUpdate_60(ref Vector3 velocity)
		{
			base.ConfigFixedUpdate_60(ref velocity);
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
				speed += 1.2f * num;
			}
			if (KeyBindingDefOf.MapDolly_Down.IsDown)
			{
				speed -= 1.2f * num;
				if (speed < 0f)
				{
					speed = 0f;
				}
			}
			angle = Mathf.Lerp(angle, targetAngle, 0.02f);
			velocity.x = Mathf.Cos(angle) * speed;
			velocity.z = Mathf.Sin(angle) * speed;
		}
	}
}
