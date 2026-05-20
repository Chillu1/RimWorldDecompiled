using UnityEngine;

namespace Verse;

public struct FleckAttachLink
{
	private TargetInfo targetInt;

	private Vector3 lastDrawPosInt;

	public int detachAfterTicks;

	public static readonly FleckAttachLink Invalid = new FleckAttachLink(TargetInfo.Invalid);

	public bool Linked => targetInt.IsValid;

	public TargetInfo Target => targetInt;

	public Vector3 LastDrawPos => lastDrawPosInt;

	public FleckAttachLink(TargetInfo target)
	{
		targetInt = target;
		detachAfterTicks = -1;
		lastDrawPosInt = Vector3.zero;
		if (target.IsValid)
		{
			UpdateDrawPos();
		}
	}

	public void UpdateDrawPos()
	{
		if (targetInt.HasThing)
		{
			lastDrawPosInt = targetInt.Thing.DrawPosHeld ?? lastDrawPosInt;
		}
		else
		{
			lastDrawPosInt = targetInt.Cell.ToVector3Shifted();
		}
	}
}
