using UnityEngine;

namespace Verse;

public struct FleckCreationData
{
	public FleckDef def;

	public Vector3 spawnPosition;

	public float rotation;

	public float scale;

	public Vector3? exactScale;

	public Color? instanceColor;

	public float velocityAngle;

	public float velocitySpeed;

	public Vector3? velocity;

	public float rotationRate;

	public float? solidTimeOverride;

	public float? airTimeLeft;

	public int ageTicksOverride;

	public FleckAttachLink link;

	public float targetSize;

	public float orbitSpeed;

	public float orbitSnapStrength;
}
