using UnityEngine;

namespace Verse
{
	public class MoteLeaf : Mote
	{
		private Vector3 startSpatialPosition;

		private Vector3 currentSpatialPosition;

		private float spawnDelay;

		private bool front;

		private float treeHeight;

		[TweakValue("Graphics", 0f, 5f)]
		private static float FallSpeed = 0.5f;

		protected override bool EndOfLife => base.AgeSecs >= spawnDelay + FallTime + base.SolidTime + def.mote.fadeOutTime;

		private float FallTime => startSpatialPosition.y / FallSpeed;

		public override float Alpha
		{
			get
			{
				float ageSecs = base.AgeSecs;
				if (ageSecs <= spawnDelay)
				{
					return 0f;
				}
				ageSecs -= spawnDelay;
				if (ageSecs <= def.mote.fadeInTime)
				{
					if (def.mote.fadeInTime > 0f)
					{
						return ageSecs / def.mote.fadeInTime;
					}
					return 1f;
				}
				if (ageSecs <= FallTime + base.SolidTime)
				{
					return 1f;
				}
				ageSecs -= FallTime + base.SolidTime;
				if (ageSecs <= def.mote.fadeOutTime)
				{
					return 1f - Mathf.InverseLerp(0f, def.mote.fadeOutTime, ageSecs);
				}
				ageSecs -= def.mote.fadeOutTime;
				return 0f;
			}
		}

		public void Initialize(Vector3 position, float spawnDelay, bool front, float treeHeight)
		{
			startSpatialPosition = position;
			this.spawnDelay = spawnDelay;
			this.front = front;
			this.treeHeight = treeHeight;
			TimeInterval(0f);
		}

		protected override void TimeInterval(float deltaTime)
		{
			base.TimeInterval(deltaTime);
			if (!base.Destroyed)
			{
				float ageSecs = base.AgeSecs;
				exactPosition = startSpatialPosition;
				if (ageSecs > spawnDelay)
				{
					exactPosition.y -= FallSpeed * (ageSecs - spawnDelay);
				}
				exactPosition.y = Mathf.Max(exactPosition.y, 0f);
				currentSpatialPosition = exactPosition;
				exactPosition.z += exactPosition.y;
				exactPosition.y = 0f;
			}
		}

		public override void Draw()
		{
			Draw(front ? (def.altitudeLayer.AltitudeFor() + 0.1f * GenMath.InverseLerp(0f, treeHeight, currentSpatialPosition.y) * 2f) : def.altitudeLayer.AltitudeFor());
		}
	}
}
