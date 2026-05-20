using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_SpawnerPawn : CompProperties
{
	public List<PawnKindDef> spawnablePawnKinds;

	public SoundDef spawnSound;

	[NoTranslate]
	public string spawnMessageKey;

	[NoTranslate]
	public string noPawnsLeftToSpawnKey;

	[NoTranslate]
	public string pawnsLeftToSpawnKey;

	public bool showNextSpawnInInspect;

	public bool shouldJoinParentLord;

	public Type lordJob;

	public float defendRadius = 21f;

	public float lordJoinRadius = 2.1474836E+09f;

	public int initialPawnsCount;

	public float initialPawnsPoints;

	public float maxSpawnedPawnsPoints = -1f;

	public FloatRange pawnSpawnIntervalDays = new FloatRange(0.85f, 1.15f);

	public int pawnSpawnRadius = 2;

	public IntRange maxPawnsToSpawn = IntRange.Zero;

	public bool chooseSingleTypeToSpawn;

	[NoTranslate]
	public string nextSpawnInspectStringKey;

	[NoTranslate]
	public string nextSpawnInspectStringKeyDormant;

	public CompProperties_SpawnerPawn()
	{
		compClass = typeof(CompSpawnerPawn);
	}
}
