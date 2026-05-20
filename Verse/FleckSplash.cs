using UnityEngine;

namespace Verse;

public struct FleckSplash : IFleck
{
	public const float VelocityFootstep = 1.5f;

	public const float SizeFootstep = 2f;

	public const float VelocityGunfire = 4f;

	public const float SizeGunfire = 1f;

	public const float VelocityExplosion = 20f;

	public const float SizeExplosion = 6f;

	public FleckDef def;

	private float ageSecs;

	private int setupTick;

	private float targetSize;

	private float velocity;

	private Vector3 position;

	private Vector3 spawnPosition;

	private Vector3 exactScale;

	public bool EndOfLife => ageSecs >= targetSize / velocity;

	public float Alpha
	{
		get
		{
			Mathf.Clamp01(ageSecs * 10f);
			float num = Mathf.Clamp01(1f - ageSecs / (targetSize / velocity));
			return 1f * num * CalculatedIntensity();
		}
	}

	public bool TimeInterval(float deltaTime, Map map)
	{
		if (EndOfLife)
		{
			return true;
		}
		ageSecs += deltaTime;
		if (def.growthRate != 0f)
		{
			exactScale = new Vector3(exactScale.x + def.growthRate * deltaTime, exactScale.y, exactScale.z + def.growthRate * deltaTime);
			exactScale.x = Mathf.Max(exactScale.x, 0.0001f);
			exactScale.z = Mathf.Max(exactScale.z, 0.0001f);
		}
		float num = ageSecs * velocity;
		exactScale = Vector3.one * num;
		position += map.waterInfo.GetWaterMovement(position) * deltaTime;
		return false;
	}

	public void Draw(DrawBatch batch)
	{
		position.y = def.altitudeLayer.AltitudeFor(def.altitudeLayerIncOffset);
		int num = setupTick + spawnPosition.GetHashCode();
		((Graphic_Fleck)def.GetGraphicData(num).Graphic).DrawFleck(new FleckDrawData
		{
			alpha = Alpha,
			color = Color.white,
			drawLayer = 0,
			pos = position,
			rotation = 0f,
			scale = exactScale,
			ageSecs = ageSecs,
			calculatedShockwaveSpan = CalculatedShockwaveSpan(),
			id = num
		}, batch);
	}

	public void Setup(FleckCreationData creationData)
	{
		def = creationData.def;
		position = creationData.spawnPosition;
		spawnPosition = creationData.spawnPosition;
		velocity = creationData.velocitySpeed;
		targetSize = creationData.targetSize;
		setupTick = Find.TickManager.TicksGame;
		if (creationData.ageTicksOverride != -1)
		{
			ForceSpawnTick(creationData.ageTicksOverride);
		}
	}

	public float CalculatedIntensity()
	{
		return Mathf.Sqrt(targetSize) / 10f;
	}

	public float CalculatedShockwaveSpan()
	{
		return Mathf.Min(Mathf.Sqrt(targetSize) * 0.8f, exactScale.x) / exactScale.x;
	}

	public void ForceSpawnTick(int tick)
	{
		setupTick = tick;
		ageSecs = (Find.TickManager.TicksGame - tick).TicksToSeconds();
	}

	public Vector3 GetPosition()
	{
		return position;
	}
}
