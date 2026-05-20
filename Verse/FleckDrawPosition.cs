using UnityEngine;

namespace Verse;

public struct FleckDrawPosition
{
	public Vector3 worldPosition;

	public float height;

	public Vector3 anchorOffset;

	public Vector3 unattachedDrawOffset;

	public Vector3 attachedOffset;

	public Vector3 ExactPosition => worldPosition + unattachedDrawOffset + attachedOffset + Vector3.forward * height + anchorOffset;

	public FleckDrawPosition(Vector3 worldPos, float height, Vector3 anchorOffset, Vector3 unattachedOffset)
	{
		worldPosition = worldPos;
		this.height = height;
		this.anchorOffset = anchorOffset;
		unattachedDrawOffset = unattachedOffset;
		attachedOffset = Vector3.zero;
	}

	public FleckDrawPosition(Vector3 worldPos, float height, Vector3 anchorOffset, Vector3 unattachedOffset, Vector3 attachedOffset)
	{
		worldPosition = worldPos;
		this.height = height;
		this.anchorOffset = anchorOffset;
		unattachedDrawOffset = unattachedOffset;
		this.attachedOffset = attachedOffset;
	}
}
