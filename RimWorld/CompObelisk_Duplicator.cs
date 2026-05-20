using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompObelisk_Duplicator : CompObelisk_ExplodingSpawner
{
	private int outsideBlastCount;

	private int spawnedCount;

	private static List<Pawn> tmpDuplicateCandidates = new List<Pawn>();

	private const float DuplicateCombatPoints = 35f;

	private const float BlastRadius = 4.9f;

	private const float PercentSpawnedOutsideBlast = 0.7f;

	private const float CombatPointsFactor = 0.6f;

	private const int MinAgeYears = 10;

	private static readonly SimpleCurve CombatPointsSpawnPointsCurve = new SimpleCurve
	{
		new CurvePoint(500f, 50f),
		new CurvePoint(1000f, 100f),
		new CurvePoint(5000f, 200f)
	};

	public override void TriggerInteractionEffect(Pawn interactor, bool triggeredByPlayer = false)
	{
		if (!CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, 16, IsValidSpawnCell, out var result, 100))
		{
			return;
		}
		if (AnomalyUtility.TryDuplicatePawn(interactor, result, parent.Map, out var duplicatePawn, null, allowCreepjoiners: false, randomOutcome: true))
		{
			lastInteractionEffectTick = Find.TickManager.TicksGame;
			if (duplicatePawn.Faction == Faction.OfEntities)
			{
				base.Lord.AddPawn(duplicatePawn);
				return;
			}
			Find.LetterStack.ReceiveLetter("ObeliskDuplicationLetterLabel".Translate(), "ObeliskDuplicationLetter".Translate(interactor.Named("PAWN")), LetterDefOf.NeutralEvent, duplicatePawn);
			Messages.Message("ObeliskDuplicationSuccessMessage".Translate(interactor.Named("PAWN")), interactor, MessageTypeDefOf.NeutralEvent);
		}
		else if (triggeredByPlayer)
		{
			parent.GetComp<CompObeliskTriggerInteractor>().ResetCooldown(sendMessage: false);
			Find.LetterStack.ReceiveLetter("ObeliskDuplicationFailedLetterLabel".Translate(), "ObeliskDuplicationFailedLetter".Translate(interactor.Named("PAWN")), LetterDefOf.NeutralEvent, duplicatePawn);
		}
	}

	public override void OnActivityActivated()
	{
		base.OnActivityActivated();
		pointsRemaining *= 0.6f;
		int num = Mathf.CeilToInt(pointsRemaining / 35f);
		outsideBlastCount = Mathf.CeilToInt((float)num * 0.7f);
		Find.LetterStack.ReceiveLetter("DuplicatorObeliskLetterLabel".Translate(), "DuplicatorObeliskLetter".Translate(), LetterDefOf.ThreatBig, parent);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (!activated || base.ActivityComp.Deactivated || explodeTick > 0 || Find.TickManager.TicksGame < nextSpawnTick || !warmupComplete)
		{
			return;
		}
		nextSpawnTick = Find.TickManager.TicksGame + SpawnIntervalTicks.RandomInRange;
		tmpDuplicateCandidates.Clear();
		bool flag = false;
		if (Find.Storyteller.difficulty.childRaidersAllowed)
		{
			tmpDuplicateCandidates = parent.Map.mapPawns.FreeColonistsSpawned.Where((Pawn p) => p.ageTracker.AgeBiologicalYears >= 10).ToList();
			if (tmpDuplicateCandidates.Count == 0)
			{
				tmpDuplicateCandidates = Find.WorldPawns.AllPawnsAlive.Where((Pawn p) => p.ageTracker.AgeBiologicalYears >= 10).ToList();
				flag = true;
			}
		}
		else
		{
			tmpDuplicateCandidates = parent.Map.mapPawns.FreeAdultColonistsSpawned;
			if (tmpDuplicateCandidates.Count == 0)
			{
				tmpDuplicateCandidates = Find.WorldPawns.AllPawnsAlive.Where((Pawn p) => p.DevelopmentalStage.Adult()).ToList();
				flag = true;
			}
		}
		if (tmpDuplicateCandidates.Count != 0)
		{
			Pawn pawn = tmpDuplicateCandidates.RandomElement();
			IntVec3 root;
			int squareRadius;
			if (spawnedCount < outsideBlastCount)
			{
				root = (flag ? parent.Position : pawn.Position);
				squareRadius = 20;
			}
			else
			{
				root = parent.Position;
				squareRadius = Mathf.FloorToInt(4.9f);
			}
			if (CellFinder.TryFindRandomCellNear(root, parent.Map, squareRadius, IsValidSpawnCell, out var result, 100) && AnomalyUtility.TryDuplicatePawn(pawn, result, parent.Map, out var duplicatePawn, Faction.OfEntities, allowCreepjoiners: false, randomOutcome: false, negativeOutcomes: false))
			{
				base.Lord.AddPawn(duplicatePawn);
				EffecterDefOf.ObeliskSpark.Spawn(parent.Position, parent.Map).Cleanup();
				pointsRemaining -= CombatPointsSpawnPointsCurve.Evaluate(totalCombatPoints);
				spawnedCount++;
				Messages.Message("MessageHostileDuplicate".Translate(pawn), duplicatePawn, MessageTypeDefOf.ThreatBig);
			}
			if (pointsRemaining <= 0f)
			{
				PrepareExplosion();
			}
		}
	}

	private bool IsValidSpawnCell(IntVec3 pos)
	{
		if (pos.Standable(parent.Map) && pos.Walkable(parent.Map))
		{
			return !pos.Fogged(parent.Map);
		}
		return false;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref outsideBlastCount, "outsideBlastCount", 0);
		Scribe_Values.Look(ref spawnedCount, "spawnedCount", 0);
	}
}
