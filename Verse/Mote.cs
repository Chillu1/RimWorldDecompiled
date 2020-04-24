using UnityEngine;

namespace Verse
{
	public abstract class Mote : Thing
	{
		public Vector3 exactPosition;

		public float exactRotation;

		public Vector3 exactScale = new Vector3(1f, 1f, 1f);

		public float rotationRate;

		public Color instanceColor = Color.white;

		private int lastMaintainTick;

		public float solidTimeOverride = -1f;

		public int spawnTick;

		public float spawnRealTime;

		public MoteAttachLink link1 = MoteAttachLink.Invalid;

		protected float skidSpeedMultiplierPerTick = Rand.Range(0.3f, 0.95f);

		protected const float MinSpeed = 0.02f;

		public float Scale
		{
			set
			{
				exactScale = new Vector3(value, 1f, value);
			}
		}

		public float AgeSecs
		{
			get
			{
				if (def.mote.realTime)
				{
					return Time.realtimeSinceStartup - spawnRealTime;
				}
				return (float)(Find.TickManager.TicksGame - spawnTick) / 60f;
			}
		}

		protected float SolidTime
		{
			get
			{
				if (!(solidTimeOverride < 0f))
				{
					return solidTimeOverride;
				}
				return def.mote.solidTime;
			}
		}

		public override Vector3 DrawPos => exactPosition;

		protected virtual bool EndOfLife => AgeSecs >= def.mote.Lifespan;

		public virtual float Alpha
		{
			get
			{
				float ageSecs = AgeSecs;
				if (ageSecs <= def.mote.fadeInTime)
				{
					if (def.mote.fadeInTime > 0f)
					{
						return ageSecs / def.mote.fadeInTime;
					}
					return 1f;
				}
				if (ageSecs <= def.mote.fadeInTime + SolidTime)
				{
					return 1f;
				}
				if (def.mote.fadeOutTime > 0f)
				{
					return 1f - Mathf.InverseLerp(def.mote.fadeInTime + SolidTime, def.mote.fadeInTime + SolidTime + def.mote.fadeOutTime, ageSecs);
				}
				return 1f;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			spawnTick = Find.TickManager.TicksGame;
			spawnRealTime = Time.realtimeSinceStartup;
			RealTime.moteList.MoteSpawned(this);
			base.Map.moteCounter.Notify_MoteSpawned();
			exactPosition.y = def.altitudeLayer.AltitudeFor();
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			base.DeSpawn(mode);
			RealTime.moteList.MoteDespawned(this);
			map.moteCounter.Notify_MoteDespawned();
		}

		public override void Tick()
		{
			if (!def.mote.realTime)
			{
				TimeInterval(0.0166666675f);
			}
		}

		public void RealtimeUpdate()
		{
			if (def.mote.realTime)
			{
				TimeInterval(Time.deltaTime);
			}
		}

		protected virtual void TimeInterval(float deltaTime)
		{
			if (EndOfLife && !base.Destroyed)
			{
				Destroy();
			}
			else if (def.mote.needsMaintenance && Find.TickManager.TicksGame - 1 > lastMaintainTick)
			{
				Destroy();
			}
			else if (def.mote.growthRate != 0f)
			{
				exactScale = new Vector3(exactScale.x + def.mote.growthRate * deltaTime, exactScale.y, exactScale.z + def.mote.growthRate * deltaTime);
				exactScale.x = Mathf.Max(exactScale.x, 0.0001f);
				exactScale.z = Mathf.Max(exactScale.z, 0.0001f);
			}
		}

		public override void Draw()
		{
			Draw(def.altitudeLayer.AltitudeFor());
		}

		public void Draw(float altitude)
		{
			exactPosition.y = altitude;
			base.Draw();
		}

		public void Maintain()
		{
			lastMaintainTick = Find.TickManager.TicksGame;
		}

		public void Attach(TargetInfo a)
		{
			link1 = new MoteAttachLink(a);
		}

		public override void Notify_MyMapRemoved()
		{
			base.Notify_MyMapRemoved();
			RealTime.moteList.MoteDespawned(this);
		}
	}
}
