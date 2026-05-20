using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_SpawnPawnsOnDamaged : CompProperties
{
	public float chance = 1f;

	public int cooldownTicks;

	public IntRange spawnPawnCountRange = new IntRange(1, 1);

	public List<PawnKindDef> spawnPawnKindOptions = new List<PawnKindDef>();

	public CompProperties_SpawnPawnsOnDamaged()
	{
		compClass = typeof(CompSpawnPawnsOnDamaged);
	}
}
