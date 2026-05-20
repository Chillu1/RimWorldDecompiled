using UnityEngine;

namespace Verse;

public class SubEffecter
{
	public Effecter parent;

	public SubEffecterDef def;

	public Color? colorOverride;

	public MoteSpawnLocType? spawnLocOverride;

	public Vector3? dimensionsOverride;

	public Vector3? offsetOverride;

	public float? chanceOverride;

	public Color EffectiveColor => colorOverride ?? def.color;

	public MoteSpawnLocType EffectiveSpawnLocType => spawnLocOverride ?? def.spawnLocType;

	public Vector3? EffectiveDimensions => dimensionsOverride ?? def.positionDimensions;

	public Vector3 EffectiveOffset => offsetOverride ?? def.positionOffset;

	public float EffectiveChancePerTick => chanceOverride ?? def.chancePerTick;

	public SubEffecter(SubEffecterDef subDef, Effecter parent)
	{
		def = subDef;
		this.parent = parent;
	}

	public virtual void SubEffectTick(TargetInfo A, TargetInfo B)
	{
	}

	public virtual void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
	{
	}

	public virtual void SubCleanup()
	{
	}
}
