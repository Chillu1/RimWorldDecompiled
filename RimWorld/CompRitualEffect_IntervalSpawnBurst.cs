using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompRitualEffect_IntervalSpawnBurst : CompRitualEffect_IntervalSpawn
	{
		public override Mote SpawnMote(LordJob_Ritual ritual, Vector3? forcedPos = null)
		{
			for (int i = 0; i < base.Props.spawnCount; i++)
			{
				base.SpawnMote(ritual);
			}
			lastSpawnTick = GenTicks.TicksGame;
			burstsDone++;
			return null;
		}

		public override void SpawnFleck(LordJob_Ritual ritual, Vector3? forcedPos = null, float? exactRotation = null)
		{
			for (int i = 0; i < base.Props.spawnCount; i++)
			{
				base.SpawnFleck(ritual);
			}
			lastSpawnTick = GenTicks.TicksGame;
			burstsDone++;
		}
	}
}
