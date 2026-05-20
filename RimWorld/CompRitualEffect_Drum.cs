using UnityEngine;
using Verse;

namespace RimWorld;

public class CompRitualEffect_Drum : CompRitualEffect_IntervalSpawn
{
	protected new CompProperties_RitualEffectDrum Props => (CompProperties_RitualEffectDrum)props;

	protected override Vector3 SpawnPos(LordJob_Ritual ritual)
	{
		return Vector3.zero;
	}

	public override void SpawnFleck(LordJob_Ritual ritual, Vector3? forcedPos = null, float? exactRotation = null)
	{
		foreach (Thing item in ritual.Map.listerBuldingOfDefInProximity.GetForCell(ritual.selectedTarget.Cell, Props.maxDistance, ThingDefOf.Drum))
		{
			if (item is Building_MusicalInstrument building_MusicalInstrument && item.GetRoom() == ritual.selectedTarget.Cell.GetRoom(ritual.Map) && building_MusicalInstrument.IsBeingPlayed)
			{
				for (int i = 0; i < Props.spawnCount; i++)
				{
					float num = Rand.Sign;
					float num2 = Rand.Sign;
					Vector3 vector = new Vector3(num * Rand.Value * Props.maxOffset, 0f, num2 * Rand.Value * Props.maxOffset);
					base.SpawnFleck(ritual, item.Position.ToVector3Shifted() + vector);
				}
			}
		}
		burstsDone++;
		lastSpawnTick = GenTicks.TicksGame;
	}
}
