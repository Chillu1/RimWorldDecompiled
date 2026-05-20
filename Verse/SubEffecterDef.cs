using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class SubEffecterDef
{
	public Type subEffecterClass;

	public IntRange burstCount = new IntRange(1, 1);

	public int ticksBetweenMotes = 40;

	public int maxMoteCount = int.MaxValue;

	public int initialDelayTicks;

	public int lifespanMaxTicks = 999999;

	public float chancePerTick = 0.1f;

	public int chancePeriodTicks;

	public MoteSpawnLocType spawnLocType = MoteSpawnLocType.BetweenPositions;

	public float positionLerpFactor = 0.5f;

	public Vector3 positionOffset = Vector3.zero;

	public float positionRadius;

	public float positionRadiusMin;

	public List<Vector3> perRotationOffsets;

	public Vector3? positionDimensions;

	public bool attachToSpawnThing;

	public float avoidLastPositionRadius;

	public AttachPointType attachPoint;

	public ThingDef moteDef;

	public FleckDef fleckDef;

	public Color color = Color.white;

	public FloatRange angle = new FloatRange(0f, 360f);

	public bool absoluteAngle;

	public bool useTargetAInitialRotation;

	public bool useTargetBInitialRotation;

	public bool fleckUsesAngleForVelocity;

	public bool rotateTowardsTargetCenter;

	public bool useTargetABodyAngle;

	public bool useTargetBBodyAngle;

	public FloatRange speed = new FloatRange(0f, 0f);

	public FloatRange rotation = new FloatRange(0f, 360f);

	public FloatRange rotationRate = new FloatRange(0f, 0f);

	public FloatRange scale = new FloatRange(1f, 1f);

	public FloatRange airTime = new FloatRange(999999f, 999999f);

	public SoundDef soundDef;

	public IntRange intermittentSoundInterval = new IntRange(300, 600);

	public int ticksBeforeSustainerStart;

	public bool orbitOrigin;

	public FloatRange orbitSpeed;

	public float orbitSnapStrength;

	public bool makeMoteOnSubtrigger;

	public bool destroyMoteOnCleanup;

	public FloatRange cameraShake;

	public float distanceAttenuationScale;

	public float distanceAttenuationMax = 100f;

	public float randomWeight = 1f;

	public bool subTriggerOnSpawn = true;

	public List<SubEffecterDef> children;

	public SubEffecter Spawn(Effecter parent)
	{
		return (SubEffecter)Activator.CreateInstance(subEffecterClass, this, parent);
	}
}
