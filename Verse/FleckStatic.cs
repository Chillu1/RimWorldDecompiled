using UnityEngine;

namespace Verse;

public struct FleckStatic : IFleck
{
	public FleckDef def;

	public Map map;

	public FleckDrawPosition position;

	public float exactRotation;

	public Vector3 originalScale;

	public Vector3 linearScale;

	public Vector3 curvedScale;

	public Color instanceColor;

	public float solidTimeOverride;

	public float ageSecs;

	public int ageTicks;

	public int setupTick;

	public Vector3 spawnPosition;

	public float skidSpeedMultiplierPerTick;

	public float SolidTime
	{
		get
		{
			if (!(solidTimeOverride < 0f))
			{
				return solidTimeOverride;
			}
			return def.solidTime;
		}
	}

	public Vector3 DrawPos => position.ExactPosition;

	public float Lifespan => def.fadeInTime + SolidTime + def.fadeOutTime;

	public bool EndOfLife => ageSecs >= Lifespan;

	public float Alpha
	{
		get
		{
			float num = ageSecs;
			if (num <= def.fadeInTime)
			{
				if (def.fadeInTime > 0f)
				{
					return num / def.fadeInTime;
				}
				return 1f;
			}
			if (num <= def.fadeInTime + SolidTime)
			{
				return 1f;
			}
			if (def.fadeOutTime > 0f)
			{
				return 1f - Mathf.InverseLerp(def.fadeInTime + SolidTime, def.fadeInTime + SolidTime + def.fadeOutTime, num);
			}
			return 1f;
		}
	}

	public Vector3 ExactScale => Vector3.Scale(linearScale, curvedScale);

	public Vector3 AddedScale => ExactScale - originalScale;

	public void Setup(FleckCreationData creationData)
	{
		def = creationData.def;
		linearScale = Vector3.one;
		instanceColor = creationData.instanceColor ?? Color.white;
		solidTimeOverride = creationData.solidTimeOverride ?? (-1f);
		skidSpeedMultiplierPerTick = Rand.Range(0.3f, 0.95f);
		ageSecs = 0f;
		if (creationData.exactScale.HasValue)
		{
			linearScale = creationData.exactScale.Value;
		}
		else
		{
			linearScale = new Vector3(creationData.scale, 1f, creationData.scale);
		}
		originalScale = ExactScale;
		position = new FleckDrawPosition(creationData.spawnPosition, 0f, Vector3.zero, def.unattachedDrawOffset);
		spawnPosition = creationData.spawnPosition;
		exactRotation = creationData.rotation;
		setupTick = Find.TickManager.TicksGame;
		curvedScale = def.scalers?.ScaleAtTime(0f) ?? Vector3.one;
		if (creationData.ageTicksOverride != -1)
		{
			ForceSpawnTick(creationData.ageTicksOverride);
		}
	}

	public bool TimeInterval(float deltaTime, Map map)
	{
		if (EndOfLife)
		{
			return true;
		}
		ageSecs += deltaTime;
		ageTicks++;
		if (def.growthRate != 0f)
		{
			float num = Mathf.Sign(linearScale.x);
			float num2 = Mathf.Sign(linearScale.z);
			linearScale = new Vector3(linearScale.x + num * (def.growthRate * deltaTime), linearScale.y, linearScale.z + num2 * (def.growthRate * deltaTime));
			linearScale.x = ((num > 0f) ? Mathf.Max(linearScale.x, 0.0001f) : Mathf.Min(linearScale.x, -0.0001f));
			linearScale.z = ((num2 > 0f) ? Mathf.Max(linearScale.z, 0.0001f) : Mathf.Min(linearScale.z, -0.0001f));
		}
		if (def.scalers != null)
		{
			curvedScale = def.scalers.ScaleAtTime(ageSecs);
		}
		return false;
	}

	public void Draw(DrawBatch batch)
	{
		Draw(def.altitudeLayer.AltitudeFor(def.altitudeLayerIncOffset), batch);
	}

	public void Draw(float altitude, DrawBatch batch)
	{
		position.worldPosition.y = altitude;
		int num = setupTick + spawnPosition.GetHashCode();
		((Graphic_Fleck)def.GetGraphicData(num).Graphic).DrawFleck(new FleckDrawData
		{
			alpha = Alpha,
			color = instanceColor,
			drawLayer = 0,
			pos = DrawPos,
			rotation = exactRotation,
			scale = ExactScale,
			ageSecs = ageSecs,
			id = num
		}, batch);
	}

	public void ForceSpawnTick(int tick)
	{
		ageTicks = Find.TickManager.TicksGame - tick;
		ageSecs = ageTicks.TicksToSeconds();
	}

	public Vector3 GetPosition()
	{
		return position.worldPosition;
	}
}
