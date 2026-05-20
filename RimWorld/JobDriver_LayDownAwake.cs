using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_LayDownAwake : JobDriver_LayDown
{
	public override bool CanSleep => false;

	public override bool CanRest => false;

	public override bool LookForOtherJobs => true;

	public override Rot4 ForcedLayingRotation => job.GetTarget(TargetIndex.A).Thing?.Rotation.Rotated(RotationDirection.Clockwise) ?? base.ForcedLayingRotation;

	public override Vector3 ForcedBodyOffset
	{
		get
		{
			Thing thing = job.GetTarget(TargetIndex.A).Thing;
			if (thing != null && thing.def.Size.z > 1)
			{
				return new Vector3(0f, 0f, 0.5f).RotatedBy(thing.Rotation);
			}
			return base.ForcedBodyOffset;
		}
	}

	public override string GetReport()
	{
		return "ReportLayingDown".Translate();
	}
}
