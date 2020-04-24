using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_SpawnerPawn : CompProperties
	{
		public List<PawnKindDef> spawnablePawnKinds;

		public SoundDef spawnSound;

		public string spawnMessageKey;

		public bool showNextSpawnInInspect;

		public bool shouldJoinParentLord;

		public Type lordJob;

		public float defendRadius = 21f;

		public int initialPawnsCount;

		public float initialPawnsPoints;

		public float maxSpawnedPawnsPoints = -1f;

		public FloatRange pawnSpawnIntervalDays = new FloatRange(0.85f, 1.15f);

		public int pawnSpawnRadius = 2;

		public bool chooseSingleTypeToSpawn;

		public string nextSpawnInspectStringKey;

		public CompProperties_SpawnerPawn()
		{
			compClass = typeof(CompSpawnerPawn);
		}
	}
}
