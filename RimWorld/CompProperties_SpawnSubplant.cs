using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_SpawnSubplant : CompProperties
{
	public ThingDef subplant;

	[NoTranslate]
	public string saveKeysPrefix;

	public SoundDef spawnSound;

	public float maxRadius;

	public float subplantSpawnDays;

	public float minGrowthForSpawn = 0.2f;

	public FloatRange? initialGrowthRange;

	public bool canSpawnOverPlayerSownPlants = true;

	public List<ThingDef> plantsToNotOverwrite;

	public SimpleCurve chanceOverDistance;

	public ThingDef dontWipePlant;

	public int maxPlants = -1;

	public int checkRespawnIntervalHours = 18;
}
