using UnityEngine;

namespace Verse;

public struct MoteAttachLink
{
	private TargetInfo targetInt;

	private Vector3 offsetInt;

	private Vector3 lastDrawPosInt;

	public bool rotateWithTarget;

	public static readonly MoteAttachLink Invalid = new MoteAttachLink(TargetInfo.Invalid, Vector3.zero);

	public bool Linked => targetInt.IsValid;

	public TargetInfo Target => targetInt;

	public Vector3 LastDrawPos => lastDrawPosInt;

	public MoteAttachLink(TargetInfo target, Vector3 offset, bool rotateWithTarget = false)
	{
		targetInt = target;
		offsetInt = offset;
		this.rotateWithTarget = rotateWithTarget;
		lastDrawPosInt = Vector3.zero;
		if (target.IsValid)
		{
			UpdateDrawPos();
		}
	}

	public void UpdateTarget(TargetInfo target, Vector3 offset)
	{
		targetInt = target;
		offsetInt = offset;
	}

	public Vector3 GetOffset()
	{
		if (!rotateWithTarget || !targetInt.HasThing || !targetInt.Thing.SpawnedOrAnyParentSpawned)
		{
			return offsetInt;
		}
		return offsetInt.RotatedBy(targetInt.Thing.Rotation);
	}

	public void UpdateDrawPos()
	{
		if (targetInt.HasThing && targetInt.Thing.SpawnedOrAnyParentSpawned)
		{
			lastDrawPosInt = targetInt.Thing.SpawnedParentOrMe.DrawPos + GetOffset();
		}
		else
		{
			lastDrawPosInt = targetInt.Cell.ToVector3Shifted() + GetOffset();
		}
	}
}
