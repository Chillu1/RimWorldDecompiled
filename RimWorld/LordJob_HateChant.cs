using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class LordJob_HateChant : LordJob
{
	private static readonly SimpleCurve MinDistBetweenChantersCurve = new SimpleCurve
	{
		new CurvePoint(0f, 6f),
		new CurvePoint(30f, 5f),
		new CurvePoint(50f, 4f),
		new CurvePoint(70f, 3f)
	};

	public bool spawningFinished;

	public bool spawningStarted;

	public bool smallGroup;

	private int lostPawns;

	private int assaultLossThreshold;

	private HashSet<IntVec3> chantingPositions = new HashSet<IntVec3>();

	private List<PsychicRitualParticipant> participants = new List<PsychicRitualParticipant>();

	private List<Sustainer> sustainers = new List<Sustainer>();

	private readonly List<Mote> motes = new List<Mote>();

	private readonly Dictionary<IntVec3, Mote> callMotes = new Dictionary<IntVec3, Mote>();

	private int nextCallTick;

	private const string SpawnFinishedSignal = "PositionsReady";

	private const int SpawningBatchSize = 5;

	private const float SpawningIntervalSeconds = 0.8f;

	private const int MaxAssaultLossThreshold = 10;

	private const int SustainerDistFromEdge = 15;

	private const int GotoToilTimeout = 10800;

	private const int CallMoteCleanupInterval = 300;

	private static readonly IntRange MinDistFromEdgeRange = new IntRange(5, 25);

	private static readonly FloatRange AssaultLossPercent = new FloatRange(0.1f, 0.3f);

	private static readonly FloatRange PawnWaitBeforeAssaultSeconds = new FloatRange(0.1f, 6f);

	private static readonly FloatRange CallIntervalSeconds = new FloatRange(3f, 5f);

	private static readonly FloatRange CallIntervalSecondsSmallGroup = new FloatRange(7f, 12f);

	public override bool ShouldExistWithoutPawns => spawningFinished = false;

	private int CallInterval
	{
		get
		{
			if (!smallGroup)
			{
				return CallIntervalSeconds.RandomInRange.SecondsToTicks();
			}
			return CallIntervalSecondsSmallGroup.RandomInRange.SecondsToTicks();
		}
	}

	public override StateGraph CreateGraph()
	{
		List<Pawn> list = new List<Pawn>();
		if (!spawningStarted)
		{
			list.AddRange(lord.ownedPawns);
			for (int i = 0; i < list.Count; i++)
			{
				lord.RemovePawn(list[i]);
			}
			spawningStarted = true;
		}
		assaultLossThreshold = Mathf.Min(Mathf.CeilToInt(AssaultLossPercent.RandomInRange * (float)(lord.ownedPawns.Count + list.Count)), 10);
		StateGraph stateGraph = new StateGraph();
		LordToil_SpawnHateChantersDelayed lordToil_SpawnHateChantersDelayed = new LordToil_SpawnHateChantersDelayed(list, "PositionsReady", 5, 0.8f);
		stateGraph.AddToil(lordToil_SpawnHateChantersDelayed);
		LordToil_PsychicRitualParticipantGoto traversalToil = new LordToil_PsychicRitualParticipantGoto(10800);
		stateGraph.AddToil(traversalToil);
		Transition transition = new Transition(lordToil_SpawnHateChantersDelayed, traversalToil);
		transition.AddTrigger(new Trigger_Signal("PositionsReady"));
		transition.AddPreAction(new TransitionAction_Custom((Action)delegate
		{
			participants = GenerateParticipants().ToList();
			traversalToil.SetParticipants(participants);
			spawningFinished = true;
		}));
		stateGraph.AddTransition(transition);
		LordToil_HateChant chantToil = new LordToil_HateChant();
		stateGraph.AddToil(chantToil);
		Transition transition2 = new Transition(traversalToil, chantToil);
		transition2.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && (traversalToil.AllPawnsArrived || traversalToil.TimedOut)));
		transition2.AddPreAction(new TransitionAction_Custom((Action)delegate
		{
			chantToil.SetParticipants(participants);
		}));
		stateGraph.AddTransition(transition2);
		LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony();
		stateGraph.AddToil(lordToil_AssaultColony);
		Transition transition3 = new Transition(chantToil, lordToil_AssaultColony);
		transition3.AddTrigger(new Trigger_Custom((TriggerSignal signal) => signal.type == TriggerSignalType.Tick && lostPawns >= assaultLossThreshold));
		transition3.AddPostAction(new TransitionAction_Custom((Action)delegate
		{
			foreach (Sustainer sustainer in sustainers)
			{
				sustainer.End();
			}
			sustainers.Clear();
			Find.LetterStack.ReceiveLetter("HateChantersAttackingLabel".Translate(), "HateChantersAttackingText".Translate(), LetterDefOf.ThreatBig);
			EnableSelfResurrect();
			MakePawnsWait();
		}));
		stateGraph.AddTransition(transition3);
		return stateGraph;
	}

	public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
	{
		base.Notify_PawnLost(p, condition);
		switch (condition)
		{
		case PawnLostCondition.Killed:
			lostPawns++;
			p.addCorpseToLord = true;
			break;
		case PawnLostCondition.Incapped:
			lostPawns++;
			break;
		}
		motes.RemoveWhere((Mote m) => m.link1.Target == p);
		if (lostPawns == 1 && assaultLossThreshold > 1)
		{
			Messages.Message("MessageHateChantersAbsorbed".Translate(), MessageTypeDefOf.NeutralEvent);
		}
	}

	public override void Notify_CorpseLost(Corpse c)
	{
		base.Notify_CorpseLost(c);
		lostPawns++;
	}

	public override void LordJobTick()
	{
		base.LordJobTick();
		if (lord.CurLordToil is LordToil_HateChant)
		{
			TickSustainersAndMotes();
		}
	}

	private void TickSustainersAndMotes()
	{
		if (sustainers.Count == 0)
		{
			InitializeSfx();
			nextCallTick = Find.TickManager.TicksGame + CallInterval;
			foreach (Pawn ownedPawn in lord.ownedPawns)
			{
				motes.Add(MoteMaker.MakeAttachedOverlay(ownedPawn, ThingDefOf.Mote_HateChantShadow, new Vector3(0f, 0f, 0f)));
				motes.Add(MoteMaker.MakeAttachedOverlay(ownedPawn, ThingDefOf.Mote_PsychicRitualInvocation, new Vector3(0f, 0f, 0f)));
			}
		}
		if (lord.ownedPawns.Count > 0)
		{
			if (base.Map.IsHashIntervalTick(300))
			{
				callMotes.RemoveAll((KeyValuePair<IntVec3, Mote> m) => m.Value.Destroyed);
			}
			if (Find.TickManager.TicksGame == nextCallTick)
			{
				Pawn pawn = lord.ownedPawns.RandomElement();
				if (!callMotes.ContainsKey(pawn.Position))
				{
					SoundDefOf.Pawn_HateChanter_Call.PlayOneShot(SoundInfo.InMap(new TargetInfo(pawn.PositionHeld, pawn.MapHeld)));
					Mote value = MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_HateChantCall, Vector3.zero);
					callMotes.Add(pawn.Position, value);
				}
				nextCallTick = Find.TickManager.TicksGame + CallInterval;
			}
			foreach (Mote mote in motes)
			{
				mote?.Maintain();
			}
		}
		foreach (Sustainer sustainer in sustainers)
		{
			if (sustainer != null && !sustainer.Ended)
			{
				sustainer.Maintain();
			}
		}
	}

	private IEnumerable<PsychicRitualParticipant> GenerateParticipants()
	{
		chantingPositions.Clear();
		List<Pawn> pawns = lord.ownedPawns;
		int minDistanceBetween = Mathf.RoundToInt(MinDistBetweenChantersCurve.Evaluate(pawns.Count));
		int i = 0;
		while (i < pawns.Count)
		{
			yield return new PsychicRitualParticipant((pawns[i], GenerateChantingPosition(pawns[i], minDistanceBetween)));
			int num = i + 1;
			i = num;
		}
		if (!DebugViewSettings.drawHateChanterPositions)
		{
			yield break;
		}
		Material mat = DebugSolidColorMats.MaterialOf(Color.green * new Color(1f, 1f, 1f, 0.4f));
		foreach (IntVec3 chantingPosition in chantingPositions)
		{
			base.Map.debugDrawer.FlashCell(chantingPosition, mat, null, 100000);
		}
	}

	private IntVec3 GenerateChantingPosition(Pawn pawn, int minDistanceBetween)
	{
		int minDistFromEdge = MinDistFromEdgeRange.RandomInRange;
		IntVec3 intVec = CellFinder.RandomClosewalkCellNear(pawn.Position, base.Map, 30, Validator);
		if (intVec.CloseToEdge(base.Map, minDistFromEdge))
		{
			intVec = CellFinder.RandomClosewalkCellNear(pawn.Position, base.Map, 30, FallbackValidator);
		}
		chantingPositions.Add(intVec);
		return intVec;
		bool FallbackValidator(IntVec3 c)
		{
			if (chantingPositions.Contains(c))
			{
				return false;
			}
			if (c.GetFirstPawn(base.Map) != null)
			{
				return false;
			}
			if (c.CloseToEdge(base.Map, 3))
			{
				return false;
			}
			return true;
		}
		bool Validator(IntVec3 c)
		{
			if (chantingPositions.Contains(c))
			{
				return false;
			}
			if ((!base.Map.TileInfo.AllowRoofedEdgeWalkIn && c.Roofed(base.Map)) || base.Map.avoidGrid[c] != 0 || c.GetFirstPawn(base.Map) != null)
			{
				return false;
			}
			foreach (IntVec3 item in CellRect.CenteredOn(c, minDistanceBetween))
			{
				if (chantingPositions.Contains(item))
				{
					return false;
				}
			}
			if (c.CloseToEdge(base.Map, minDistFromEdge))
			{
				return false;
			}
			return true;
		}
	}

	private void InitializeSfx()
	{
		nextCallTick = Find.TickManager.TicksGame + CallIntervalSeconds.RandomInRange.SecondsToTicks();
		if (smallGroup)
		{
			List<IntVec3> list = new List<IntVec3>();
			foreach (IntVec3 chantingPosition in chantingPositions)
			{
				list.Add(chantingPosition);
			}
			IntVec3 cell = Gen.AveragePosition(list).ToIntVec3().ClampInsideMap(base.Map);
			sustainers.Add(SoundDefOf.HateChant_Sighing.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(cell, base.Map), MaintenanceType.PerTick)));
			return;
		}
		(int, float) tuple = default((int, float));
		(int, float) tuple2 = default((int, float));
		(int, float) tuple3 = default((int, float));
		(int, float) tuple4 = default((int, float));
		foreach (IntVec3 chantingPosition2 in chantingPositions)
		{
			if (chantingPosition2.x < 15)
			{
				tuple3.Item1++;
				tuple3.Item2 += chantingPosition2.z;
			}
			else if (chantingPosition2.x > base.Map.Size.x - 15)
			{
				tuple4.Item1++;
				tuple4.Item2 += chantingPosition2.z;
			}
			else if (chantingPosition2.z < 15)
			{
				tuple.Item1++;
				tuple.Item2 += chantingPosition2.x;
			}
			else if (chantingPosition2.z > base.Map.Size.z - 15)
			{
				tuple2.Item1++;
				tuple2.Item2 += chantingPosition2.x;
			}
		}
		if (tuple.Item1 > 0)
		{
			tuple.Item2 /= tuple.Item1;
			IntVec3 cell2 = new IntVec3(Mathf.RoundToInt(tuple.Item2), 0, 15).ClampInsideMap(base.Map);
			sustainers.Add(SoundDefOf.HateChant_Sighing.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(cell2, base.Map), MaintenanceType.PerTick)));
		}
		if (tuple2.Item1 > 0)
		{
			tuple2.Item2 /= tuple2.Item1;
			IntVec3 cell3 = new IntVec3(Mathf.RoundToInt(tuple2.Item2), 0, base.Map.Size.z - 15).ClampInsideMap(base.Map);
			sustainers.Add(SoundDefOf.HateChant_Sighing.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(cell3, base.Map), MaintenanceType.PerTick)));
		}
		if (tuple3.Item1 > 0)
		{
			tuple3.Item2 /= tuple3.Item1;
			IntVec3 cell4 = new IntVec3(15, 0, Mathf.RoundToInt(tuple3.Item2)).ClampInsideMap(base.Map);
			sustainers.Add(SoundDefOf.HateChant_Sighing.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(cell4, base.Map), MaintenanceType.PerTick)));
		}
		if (tuple4.Item1 > 0)
		{
			tuple4.Item2 /= tuple4.Item1;
			IntVec3 cell5 = new IntVec3(base.Map.Size.x - 15, 0, Mathf.RoundToInt(tuple4.Item2)).ClampInsideMap(base.Map);
			sustainers.Add(SoundDefOf.HateChant_Sighing.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(cell5, base.Map), MaintenanceType.PerTick)));
		}
	}

	private void EnableSelfResurrect()
	{
		List<Pawn> ownedPawns = lord.ownedPawns;
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			Hediff_DeathRefusal firstHediff = ownedPawns[i].health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>();
			if (firstHediff != null)
			{
				firstHediff.AIEnabled = true;
			}
		}
		List<Corpse> ownedCorpses = lord.ownedCorpses;
		for (int j = 0; j < ownedCorpses.Count; j++)
		{
			Hediff_DeathRefusal firstHediff2 = ownedCorpses[j].InnerPawn.health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>();
			if (firstHediff2 != null)
			{
				firstHediff2.AIEnabled = true;
				firstHediff2.Notify_PawnDied(null);
			}
		}
	}

	private void MakePawnsWait()
	{
		List<Pawn> ownedPawns = lord.ownedPawns;
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			PawnUtility.ForceWait(ownedPawns[i], PawnWaitBeforeAssaultSeconds.RandomInRange.SecondsToTicks());
		}
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref lostPawns, "lostPawns", 0);
		Scribe_Values.Look(ref assaultLossThreshold, "assaultLossThreshold", 0);
		Scribe_Values.Look(ref spawningFinished, "spawningFinished", defaultValue: false);
		Scribe_Values.Look(ref spawningStarted, "spawningStarted", defaultValue: false);
		Scribe_Values.Look(ref smallGroup, "smallGroup", defaultValue: false);
		Scribe_Collections.Look(ref chantingPositions, "chantingPositions", LookMode.Value);
	}
}
