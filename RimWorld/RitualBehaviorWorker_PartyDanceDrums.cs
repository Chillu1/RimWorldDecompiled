using System;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class RitualBehaviorWorker_PartyDanceDrums : RitualBehaviorWorker
{
	private Sustainer soundPlaying;

	private int numActiveDrums = -1;

	private bool anyDrumReached;

	public override Sustainer SoundPlaying => soundPlaying;

	public override bool ChecksReservations => false;

	public RitualBehaviorWorker_PartyDanceDrums()
	{
	}

	public RitualBehaviorWorker_PartyDanceDrums(RitualBehaviorDef def)
		: base(def)
	{
	}

	protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
	{
		return new LordJob_PartyDanceDrums(target, ritual, obligation, def.stages, assignments, organizer);
	}

	public override void Tick(LordJob_Ritual ritual)
	{
		base.Tick(ritual);
		TargetInfo selectedTarget = ritual.selectedTarget;
		if (Find.TickManager.TicksGame % 20 == 0 || numActiveDrums == -1)
		{
			numActiveDrums = 0;
			foreach (Thing item in selectedTarget.Map.listerBuldingOfDefInProximity.GetForCell(selectedTarget.Cell, def.maxEnhancerDistance, ThingDefOf.Drum))
			{
				Building_MusicalInstrument building_MusicalInstrument = item as Building_MusicalInstrument;
				if (item.GetRoom() == selectedTarget.Thing.GetRoom() && building_MusicalInstrument != null && building_MusicalInstrument.IsBeingPlayed)
				{
					numActiveDrums++;
					anyDrumReached = true;
				}
			}
		}
		bool flag = numActiveDrums > 0 || anyDrumReached;
		SoundDef soundDef = ((!def.soundDefsPerEnhancerCount.NullOrEmpty() && flag) ? def.soundDefsPerEnhancerCount[Math.Min(numActiveDrums, def.soundDefsPerEnhancerCount.Count - 1)] : null);
		if (soundDef != null && (soundPlaying == null || soundPlaying.def != soundDef))
		{
			soundPlaying = soundDef.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(selectedTarget.Cell, selectedTarget.Map), MaintenanceType.PerTick));
		}
		if (flag)
		{
			soundPlaying?.Maintain();
		}
	}
}
