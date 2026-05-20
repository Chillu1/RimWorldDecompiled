using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class LordToil_PartyDanceDrums : LordToil_Ritual
{
	public new LordToilData_PartyDanceDrums Data => (LordToilData_PartyDanceDrums)data;

	public LordToil_PartyDanceDrums(IntVec3 spot, LordJob_Ritual ritual, RitualStage stage, Pawn organizer)
		: base(spot, ritual, stage, organizer)
	{
		data = new LordToilData_PartyDanceDrums();
	}

	public override void UpdateAllDuties()
	{
		reservedThings.Clear();
		for (int num = lord.ownedPawns.Count - 1; num >= 0; num--)
		{
			Pawn pawn = lord.ownedPawns[num];
			pawn.mindState.duty = DutyForPawn(pawn);
			pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
	}

	private PawnDuty DutyForPawn(Pawn pawn)
	{
		DutyDef def = stage.defaultDuty;
		IntVec3 intVec = spot;
		LocalTargetInfo focusSecond = LocalTargetInfo.Invalid;
		int num = ritual.assignments.Participants.IndexOf(pawn);
		if (num != -1 && (num % 2 == 0 || ritual.assignments.Participants.Count == 0))
		{
			Building_MusicalInstrument building_MusicalInstrument = pawn.Map.listerBuildings.AllBuildingsColonistOfClass<Building_MusicalInstrument>().Where(delegate(Building_MusicalInstrument m)
			{
				if (!GatheringsUtility.InGatheringArea(m.InteractionCell, spot, pawn.Map))
				{
					return false;
				}
				return GatheringWorker_Concert.InstrumentAccessible(m, pawn) ? true : false;
			}).RandomElementWithFallback();
			if (building_MusicalInstrument != null && building_MusicalInstrument.Spawned)
			{
				def = DutyDefOf.PlayTargetInstrument;
				focusSecond = building_MusicalInstrument;
				ritual.usedThings.Add(building_MusicalInstrument);
				reservedThings.Add(building_MusicalInstrument);
				Data.playedInstruments.SetOrAdd(pawn, building_MusicalInstrument);
			}
		}
		return new PawnDuty(def, intVec, focusSecond, (LocalTargetInfo)ritual.selectedTarget);
	}

	public override void Notify_BuildingDespawnedOnMap(Building b)
	{
		if (b.def == ThingDefOf.Drum && Data.playedInstruments.ContainsValue(b))
		{
			Pawn key = Data.playedInstruments.First((KeyValuePair<Pawn, Building> kv) => kv.Value == b).Key;
			key.mindState.duty = DutyForPawn(key);
			key.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
	}
}
