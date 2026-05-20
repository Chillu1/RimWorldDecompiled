using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Utility;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.QuestGen;

public class QuestPart_EntityArrival : QuestPart
{
	private const float BurrowMinDistFromStructure = 50f;

	private const int BurrowCount = 6;

	private static readonly IntRange FleshbeastSpawnDelayTicks = new IntRange(180, 180);

	private static readonly IntRange PitBurrowEmergenceDelayRangeTicks = new IntRange(300, 1200);

	private static readonly List<VoidAwakeningUtility.WaveType> TwistedWaveOptions = new List<VoidAwakeningUtility.WaveType>
	{
		VoidAwakeningUtility.WaveType.DevourerGorehulk,
		VoidAwakeningUtility.WaveType.ShamblerGorehulk,
		VoidAwakeningUtility.WaveType.NoctolSightstealer
	};

	private string inSignal;

	private Map map;

	private List<CellRect> structureRects;

	private Building_VoidMonolith monolith;

	private int voidAwakeningBeganTick;

	public QuestPart_EntityArrival()
	{
	}

	public QuestPart_EntityArrival(string inSignal, Map map, Building_VoidMonolith monolith, int voidAwakeningBeganTick, List<CellRect> structureRects)
	{
		this.inSignal = inSignal;
		this.map = map;
		this.monolith = monolith;
		this.voidAwakeningBeganTick = voidAwakeningBeganTick;
		this.structureRects = structureRects;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref map, "map");
		Scribe_Collections.Look(ref structureRects, "structureRects", LookMode.Value);
		Scribe_References.Look(ref monolith, "monolith");
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref voidAwakeningBeganTick, "voidAwakeningBeganTick", 0);
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag.StartsWith(inSignal))
		{
			VoidAwakeningUtility.DecodeWaveType(signal.tag, out var waveType, out var pointsFactor);
			if (waveType == VoidAwakeningUtility.WaveType.Twisted)
			{
				waveType = TwistedWaveOptions.RandomElement();
			}
			float points = StorytellerUtility.DefaultThreatPointsNow(map) * pointsFactor;
			string text = "";
			string text2 = "";
			LookTargets lookTargets = null;
			switch (waveType)
			{
			case VoidAwakeningUtility.WaveType.Fleshbeast:
				lookTargets = SpawnFleshbeastBurrows(points, structureRects);
				text = "VoidAwakeningFleshbeastBurrowLabel".Translate();
				text2 = "VoidAwakeningFleshbeastBurrowText".Translate();
				break;
			case VoidAwakeningUtility.WaveType.NoctolSightstealer:
				FireSightstealerNoctolAssault(points);
				text = "VoidAwakeningEntityArrivalLabel".Translate();
				text2 = "VoidAwakeningEntityArrivalText".Translate();
				break;
			case VoidAwakeningUtility.WaveType.ShamblerGorehulk:
				lookTargets = FireShamblerGorehulkAssault(points);
				text = "VoidAwakeningEntityArrivalLabel".Translate();
				text2 = "VoidAwakeningEntityArrivalText".Translate();
				break;
			case VoidAwakeningUtility.WaveType.DevourerGorehulk:
				lookTargets = FireDevourerGorehulkAssault(points);
				text = "VoidAwakeningEntityArrivalLabel".Translate();
				text2 = "VoidAwakeningEntityArrivalText".Translate();
				break;
			}
			if (!text.NullOrEmpty())
			{
				Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.ThreatBig, lookTargets);
			}
		}
	}

	private List<Thing> SpawnFleshbeastBurrows(float points, List<CellRect> structureRects)
	{
		List<Thing> list = new List<Thing>();
		LargeBuildingSpawnParms parms = QuestNode_Root_VoidAwakening.StructureSpawnParms.ForThing(ThingDefOf.PitBurrow);
		LargeBuildingSpawnParms parms2 = QuestNode_Root_VoidAwakening.StructureSpawnParms.ForThing(ThingDefOf.PitBurrow);
		Predicate<IntVec3> extraValidator = delegate(IntVec3 intVec)
		{
			foreach (CellRect structureRect in structureRects)
			{
				if (intVec.InHorDistOf(structureRect.CenterCell, 50f))
				{
					return false;
				}
			}
			return true;
		};
		List<CellRect> list2 = new List<CellRect>();
		list2.AddRange(structureRects);
		for (int num = 0; num < 6; num++)
		{
			if (!LargeBuildingCellFinder.TryFindCell(out var cell, map, parms, list2, extraValidator) && !LargeBuildingCellFinder.TryFindCell(out cell, map, parms2, list2))
			{
				break;
			}
			Thing thing = FleshbeastUtility.SpawnFleshbeastsFromPitBurrowEmergence(cell, map, points / 6f, PitBurrowEmergenceDelayRangeTicks, FleshbeastSpawnDelayTicks);
			list.Add(thing);
			list2.Add(thing.OccupiedRect());
		}
		return list;
	}

	private List<Pawn> FireSightstealerNoctolAssault(float points)
	{
		float points2 = Mathf.Max(points, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.SightstealersNoctols) * 1.05f);
		IncidentParms incidentParms = new IncidentParms
		{
			target = map,
			raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
			sendLetter = false,
			faction = Faction.OfEntities
		};
		if (!incidentParms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(incidentParms))
		{
			return null;
		}
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.SightstealersNoctols,
			points = points2,
			faction = Faction.OfEntities
		}).ToList();
		PawnsArrivalModeDefOf.EdgeWalkInDistributedGroups.Worker.Arrive(list, incidentParms);
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_AssaultColony(incidentParms.faction, canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false), map, list);
		return list.ToList();
	}

	private List<Pawn> FireShamblerGorehulkAssault(float points)
	{
		float points2 = Mathf.Max(points / 2f, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Shamblers) * 1.05f);
		float points3 = Mathf.Max(points / 2f, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Gorehulks) * 1.05f);
		IncidentParms incidentParms = new IncidentParms
		{
			target = map,
			raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
			sendLetter = false,
			faction = Faction.OfEntities
		};
		if (!incidentParms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(incidentParms))
		{
			return null;
		}
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Shamblers,
			points = points2,
			faction = Faction.OfEntities
		}).ToList();
		PawnsArrivalModeDefOf.EdgeWalkInDistributedGroups.Worker.Arrive(list, incidentParms);
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_ShamblerAssault(), map, list);
		List<Pawn> list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Gorehulks,
			points = points3,
			faction = Faction.OfEntities
		}).ToList();
		PawnsArrivalModeDefOf.EdgeWalkInDistributedGroups.Worker.Arrive(list2, incidentParms);
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_GorehulkAssault(), map, list2);
		return list.Concat(list2).ToList();
	}

	private List<Pawn> FireDevourerGorehulkAssault(float points)
	{
		float points2 = Mathf.Max(points / 2f, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Devourers) * 1.05f);
		float points3 = Mathf.Max(points / 2f, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Gorehulks) * 1.05f);
		IncidentParms incidentParms = new IncidentParms
		{
			target = map,
			raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
			sendLetter = false,
			faction = Faction.OfEntities
		};
		if (!incidentParms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(incidentParms))
		{
			return null;
		}
		List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Devourers,
			points = points2,
			faction = Faction.OfEntities
		}).ToList();
		PawnsArrivalModeDefOf.EdgeWalkInDistributedGroups.Worker.Arrive(list, incidentParms);
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_DevourerAssault(), map, list);
		List<Pawn> list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Gorehulks,
			points = points3,
			faction = Faction.OfEntities
		}).ToList();
		PawnsArrivalModeDefOf.EdgeWalkInDistributedGroups.Worker.Arrive(list2, incidentParms);
		LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_GorehulkAssault(), map, list2);
		return list.Concat(list2).ToList();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		map = null;
		monolith = null;
		structureRects.Clear();
	}
}
