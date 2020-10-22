using System;
using UnityEngine;

namespace Verse
{
	public class SubEffecterDef
	{
		public Type subEffecterClass;

		public IntRange burstCount = new IntRange(1, 1);

		public int ticksBetweenMotes = 40;

		public int maxMoteCount = int.MaxValue;

		public int initialDelayTicks;

		public float chancePerTick = 0.1f;

		public MoteSpawnLocType spawnLocType = MoteSpawnLocType.BetweenPositions;

		public float positionLerpFactor = 0.5f;

		public Vector3 positionOffset = Vector3.zero;

		public float positionRadius;

		public ThingDef moteDef;

		public Color color = Color.white;

		public FloatRange angle = new FloatRange(0f, 360f);

		public bool absoluteAngle;

		public FloatRange speed = new FloatRange(0f, 0f);

		public FloatRange rotation = new FloatRange(0f, 360f);

		public FloatRange rotationRate = new FloatRange(0f, 0f);

		public FloatRange scale = new FloatRange(1f, 1f);

		public FloatRange airTime = new FloatRange(999999f, 999999f);

		public SoundDef soundDef;

		public IntRange intermittentSoundInterval = new IntRange(300, 600);

		public int ticksBeforeSustainerStart;

		public SubEffecter Spawn(Effecter parent)
		{
			return (SubEffecter)Activator.CreateInstance(subEffecterClass, this, parent);
		}
	}
}
