using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_PawnSpawnOnWakeup : CompProperties
{
	public List<PawnKindDef> spawnablePawnKinds;

	public SoundDef spawnSound;

	public EffecterDef spawnEffecter;

	public Type lordJob;

	public bool shouldJoinParentLord;

	[NoTranslate]
	public string activatedMessageKey;

	public FloatRange points;

	public IntRange pawnSpawnRadius = new IntRange(2, 2);

	public bool aggressive = true;

	public bool dropInPods;

	public float defendRadius = 21f;

	public MentalStateDef mentalState;

	public bool destroyAfterSpawn;

	public CompProperties_PawnSpawnOnWakeup()
	{
		compClass = typeof(CompPawnSpawnOnWakeup);
	}
}
