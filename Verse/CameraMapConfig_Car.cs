using RimWorld;
using UnityEngine;

namespace Verse;

public class CameraMapConfig_Car : CameraMapConfig
{
	private const float SpeedChangeSpeed = 1.2f;

	private const float AngleChangeSpeed = 0.72f;

	public CameraMapConfig_Car()
	{
		dollyRateKeys = 0f;
		dollyRateScreenEdge = 0f;
		camSpeedDecayFactor = 1f;
		moveSpeedScale = 1f;
	}

	public override void ConfigFixedUpdate_60(ref Vector3 rootPos, ref Vector3 velocity)
	{
		float num = 1f / 60f;
		if (KeyBindingDefOf.MapDolly_Left.IsDown)
		{
			autoPanTargetAngle += 0.72f * num;
		}
		if (KeyBindingDefOf.MapDolly_Right.IsDown)
		{
			autoPanTargetAngle -= 0.72f * num;
		}
		if (KeyBindingDefOf.MapDolly_Up.IsDown)
		{
			autoPanSpeed += 1.2f * num;
		}
		if (KeyBindingDefOf.MapDolly_Down.IsDown)
		{
			autoPanSpeed -= 1.2f * num;
			if (autoPanSpeed < 0f)
			{
				autoPanSpeed = 0f;
			}
		}
		autoPanAngle = Mathf.Lerp(autoPanAngle, autoPanTargetAngle, 0.02f);
		base.ConfigFixedUpdate_60(ref rootPos, ref velocity);
	}
}
