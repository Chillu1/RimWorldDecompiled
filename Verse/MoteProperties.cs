using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class MoteProperties
{
	public bool realTime;

	public float fadeInTime;

	public float solidTime = 1f;

	public float fadeOutTime;

	public Vector3 acceleration = Vector3.zero;

	public float speedPerTime;

	public float growthRate;

	public bool collide;

	public float archHeight;

	public float archDuration;

	public float archStartOffset;

	public SoundDef landSound;

	public Vector3 unattachedDrawOffset = Vector3.zero;

	public Vector3 attachedDrawOffset;

	public bool needsMaintenance;

	public bool rotateTowardsTarget;

	public bool rotateTowardsMoveDirection;

	public bool scaleToConnectTargets;

	public bool attachedToHead;

	public bool fadeOutUnmaintained;

	public bool updateOffsetToMatchTargetRotation;

	public float yFightingOffsetScalar01 = 1f;

	public List<CurvedScaler> scalers;

	public float Lifespan => fadeInTime + solidTime + fadeOutTime;
}
