using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.QuestGen;

public class QuestPart_WanderingEntities : QuestPartActivable
{
	private const int SpawnCheckInterval = 2500;

	private const float MaxPointFractionToSpawnPerInterval = 1f / 24f;

	private static readonly SimpleCurve MaxPointsByCurrentThreatPoints = new SimpleCurve
	{
		new CurvePoint(500f, 750f),
		new CurvePoint(2500f, 1500f),
		new CurvePoint(5000f, 2000f),
		new CurvePoint(10000f, 3000f)
	};

	private static readonly SimpleCurve MaxPointsFactorByDaysPassed = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(5f, 0.2f)
	};

	private Map map;

	private string waveStartedSignal;

	private int waveStartedTick = -99999;

	private List<Pawn> pawns = new List<Pawn>();

	private float MaxPoints => MaxPointsByCurrentThreatPoints.Evaluate(StorytellerUtility.DefaultThreatPointsNow(map)) * MaxPointsFactorByDaysPassed.Evaluate((Find.TickManager.TicksGame - waveStartedTick).TicksToDays());

	private float CurPoints => pawns.Sum((Pawn p) => p.kindDef.combatPower);

	private IEnumerable<PawnGroupKindDef> GroupKinds
	{
		get
		{
			yield return PawnGroupKindDefOf.Gorehulks;
			yield return PawnGroupKindDefOf.Noctols;
		}
	}

	public QuestPart_WanderingEntities()
	{
	}

	public QuestPart_WanderingEntities(Map map, string waveStartedSignal)
	{
		this.map = map;
		this.waveStartedSignal = waveStartedSignal;
		inSignalEnable = waveStartedSignal;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref map, "map");
		Scribe_Values.Look(ref waveStartedSignal, "waveStartedSignal");
		Scribe_Values.Look(ref waveStartedTick, "waveStartedTick", 0);
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (pawns == null)
			{
				pawns = new List<Pawn>();
			}
			pawns.RemoveAll((Pawn p) => p == null);
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		map = null;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag != waveStartedSignal))
		{
			waveStartedTick = Find.TickManager.TicksGame;
		}
	}

	public override void QuestPartTick()
	{
		if (Find.TickManager.TicksGame % 2500 == 0)
		{
			pawns = pawns.Where((Pawn p) => !p.Dead && p.Spawned).ToList();
			if (CurPoints < MaxPoints)
			{
				SpawnEntities();
			}
		}
	}

	private void SpawnEntities()
	{
		PawnGroupKindDef groupKind = GroupKinds.RandomElement();
		float a = MaxPoints * (1f / 24f);
		a = Mathf.Max(a, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(groupKind) * 1.05f);
		IncidentParms incidentParms = new IncidentParms
		{
			target = map,
			raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn,
			sendLetter = false,
			faction = Faction.OfEntities
		};
		if (incidentParms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(incidentParms))
		{
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
			{
				groupKind = groupKind,
				points = a,
				faction = Faction.OfEntities
			}).ToList();
			PawnsArrivalModeDefOf.EdgeWalkInDistributedGroups.Worker.Arrive(list, incidentParms);
			LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_VoidAwakeningWander(), map, list);
			pawns.AddRange(list);
		}
	}
}
