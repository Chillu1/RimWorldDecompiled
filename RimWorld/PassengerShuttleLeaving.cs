using UnityEngine;
using Verse;

namespace RimWorld;

public class PassengerShuttleLeaving : FlyShipLeaving
{
	private static readonly SimpleCurve AngleCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(1f, 20f)
	};

	public override Color DrawColor => base.Contents.GetShuttle().DrawColor;

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		if (!respawningAfterLoad && !base.BeingTransportedOnGravship)
		{
			angle = GetAngle(0f, base.Rotation);
		}
	}

	protected override void GetDrawPositionAndRotation(ref Vector3 drawLoc, out float extraRotation)
	{
		extraRotation = 0f;
		angle = GetAngle(base.TimeInAnimation, base.Rotation);
		switch (base.Rotation.AsInt)
		{
		case 1:
			extraRotation += def.skyfaller.rotationCurve.Evaluate(base.TimeInAnimation);
			break;
		case 3:
			extraRotation -= def.skyfaller.rotationCurve.Evaluate(base.TimeInAnimation);
			break;
		}
		drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(base.TimeInAnimation);
	}

	public override float DrawAngle()
	{
		float num = 0f;
		switch (base.Rotation.AsInt)
		{
		case 1:
			num += def.skyfaller.rotationCurve.Evaluate(base.TimeInAnimation);
			break;
		case 3:
			num -= def.skyfaller.rotationCurve.Evaluate(base.TimeInAnimation);
			break;
		}
		return num;
	}

	private static float GetAngle(float timeInAnimation, Rot4 rotation)
	{
		return rotation.AsInt switch
		{
			1 => rotation.AsAngle - AngleCurve.Evaluate(timeInAnimation), 
			3 => rotation.AsAngle + AngleCurve.Evaluate(timeInAnimation), 
			_ => rotation.AsAngle, 
		};
	}
}
