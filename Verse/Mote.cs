using System;
using UnityEngine;

namespace Verse;

public abstract class Mote : Thing
{
	public Vector3 exactPosition;

	public float exactRotation;

	public Vector3 linearScale = new Vector3(1f, 1f, 1f);

	public Vector3 curvedScale = new Vector3(1f, 1f, 1f);

	public float rotationRate;

	public float yOffset;

	public Color instanceColor = Color.white;

	private int lastMaintainTick;

	private int currentAnimationTick;

	public float solidTimeOverride = -1f;

	public int pausedTicks;

	public bool paused;

	public int spawnTick;

	public bool animationPaused;

	public int detachAfterTicks = -1;

	public float spawnRealTime;

	public MoteAttachLink link1 = MoteAttachLink.Invalid;

	protected float skidSpeedMultiplierPerTick = Rand.Range(0.3f, 0.95f);

	public int offsetRandom = Rand.Range(0, 99999);

	protected const float MinSpeed = 0.02f;

	public float Scale
	{
		set
		{
			linearScale = new Vector3(value, 1f, value);
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
			return (float)(Find.TickManager.TicksGame - spawnTick - pausedTicks) / 60f;
		}
	}

	public float AgeSecsPausable => (float)currentAnimationTick / 60f;

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

	public Vector3 ExactScale => Vector3.Scale(linearScale, curvedScale);

	public override Vector3 DrawPos
	{
		get
		{
			float z = 0f;
			if (def.mote.archDuration > 0f && AgeSecs < def.mote.archDuration + def.mote.archStartOffset)
			{
				z = (Mathf.Cos(Mathf.Clamp01((AgeSecs + def.mote.archStartOffset) / def.mote.archDuration) * MathF.PI * 2f - MathF.PI) + 1f) / 2f * def.mote.archHeight;
			}
			int num = GetHashCode();
			if (num == int.MinValue)
			{
				num++;
			}
			float y = (float)Mathf.Abs(num) / 2.1474836E+09f * 0.03658537f * def.mote.yFightingOffsetScalar01;
			return exactPosition + def.mote.unattachedDrawOffset + new Vector3(0f, y, z);
		}
	}

	protected virtual bool EndOfLife => AgeSecs >= def.mote.Lifespan;

	public virtual float Alpha
	{
		get
		{
			float ageSecs = AgeSecs;
			if (def.mote.fadeOutUnmaintained && Find.TickManager.TicksGame - lastMaintainTick > 0)
			{
				if (def.mote.fadeOutTime > 0f)
				{
					float num = (Find.TickManager.TicksGame - lastMaintainTick).TicksToSeconds();
					return 1f - num / def.mote.fadeOutTime;
				}
				return 1f;
			}
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
		if (exactPosition == Vector3.zero)
		{
			exactPosition = base.Position.ToVector3();
			exactPosition.y = def.altitudeLayer.AltitudeFor() + yOffset;
		}
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		Map map = base.Map;
		base.DeSpawn(mode);
		RealTime.moteList.MoteDespawned(this);
		map.moteCounter.Notify_MoteDespawned();
	}

	protected override void Tick()
	{
		if (!def.mote.realTime)
		{
			TimeInterval(1f / 60f);
		}
		if (!animationPaused)
		{
			currentAnimationTick++;
		}
		if (paused)
		{
			pausedTicks++;
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
			return;
		}
		if (def.mote.needsMaintenance && Find.TickManager.TicksGame > lastMaintainTick)
		{
			int num = def.mote.fadeOutTime.SecondsToTicks();
			if (!def.mote.fadeOutUnmaintained || Find.TickManager.TicksGame - lastMaintainTick > num)
			{
				Destroy();
				return;
			}
		}
		if (def.mote.growthRate != 0f)
		{
			linearScale = new Vector3(linearScale.x + def.mote.growthRate * deltaTime, linearScale.y, linearScale.z + def.mote.growthRate * deltaTime);
			linearScale.x = Mathf.Max(linearScale.x, 0.0001f);
			linearScale.z = Mathf.Max(linearScale.z, 0.0001f);
		}
		if (def.mote.scalers != null)
		{
			curvedScale = def.mote.scalers.ScaleAtTime(AgeSecs);
		}
	}

	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		DrawMote(def.altitudeLayer.AltitudeFor());
	}

	protected void DrawMote(float altitude)
	{
		if (!paused && !Find.UIRoot.HideMotes)
		{
			exactPosition.y = altitude + yOffset;
			base.DrawAt(exactPosition);
		}
	}

	public void Maintain()
	{
		lastMaintainTick = Find.TickManager.TicksGame;
	}

	public void Attach(TargetInfo a, Vector3 offset, bool rotateWithTarget = false)
	{
		link1 = new MoteAttachLink(a, offset, rotateWithTarget);
	}

	public void Attach(TargetInfo a)
	{
		link1 = new MoteAttachLink(a, Vector3.zero);
	}

	public override void Notify_MyMapRemoved()
	{
		base.Notify_MyMapRemoved();
		RealTime.moteList.MoteDespawned(this);
	}

	public void ForceSpawnTick(int tick)
	{
		spawnTick = tick;
	}
}
