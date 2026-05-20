using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_AssaultColonyBreaching : LordToil
{
	private const int UpdateIntervalTicks = 300;

	private const float PreferMeleeChance = 0.5f;

	private const float UseSoloAttackOnTargetChance = 0.2f;

	public const float MaxRangeForShooters = 12f;

	private static readonly SimpleCurve BreachRadiusFromNumRaiders = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(120f, 2f)
	};

	private static readonly SimpleCurve WalkMarginFromNumRaiders = new SimpleCurve
	{
		new CurvePoint(0f, 2f),
		new CurvePoint(60f, 4f)
	};

	private List<Pawn> pawnsRangedDestructive = new List<Pawn>();

	private List<Pawn> pawnsMeleeDestructive = new List<Pawn>();

	private List<Pawn> pawnsRangedGeneral = new List<Pawn>();

	private List<Pawn> pawnSoloAttackers = new List<Pawn>();

	private List<Pawn> pawnsEscort = new List<Pawn>();

	private List<Pawn> pawnsLost = new List<Pawn>();

	public LordToilData_AssaultColonyBreaching Data
	{
		get
		{
			if (data == null)
			{
				data = new LordToilData_AssaultColonyBreaching(lord);
			}
			return (LordToilData_AssaultColonyBreaching)data;
		}
	}

	public override bool ForceHighStoryDanger => true;

	public override bool AllowSatisfyLongNeeds => false;

	public override void Init()
	{
		base.Init();
		LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
	}

	public override void Notify_ReachedDutyLocation(Pawn pawn)
	{
		Data.Reset();
		UpdateAllDuties();
	}

	public override void Notify_BuildingSpawnedOnMap(Building b)
	{
		Data.breachingGrid.Notify_BuildingStateChanged(b);
	}

	public override void Notify_BuildingDespawnedOnMap(Building b)
	{
		Data.breachingGrid.Notify_BuildingStateChanged(b);
	}

	public override void LordToilTick()
	{
		if (lord.ticksInToil % 300 == 0)
		{
			UpdateAllDuties();
		}
	}

	public override void UpdateAllDuties()
	{
		if (!lord.ownedPawns.Any())
		{
			return;
		}
		if (!Data.breachDest.IsValid)
		{
			Data.Reset();
			Data.preferMelee = Rand.Chance(0.5f);
			Data.breachStart = lord.ownedPawns[0].PositionHeld;
			Data.breachDest = GenAI.RandomRaidDest(Data.breachStart, base.Map);
			int breachRadius = Mathf.RoundToInt(BreachRadiusFromNumRaiders.Evaluate(lord.ownedPawns.Count));
			int walkMargin = Mathf.RoundToInt(WalkMarginFromNumRaiders.Evaluate(lord.ownedPawns.Count));
			Data.breachingGrid.CreateBreachPath(Data.breachStart, Data.breachDest, breachRadius, walkMargin, useAvoidGrid);
		}
		pawnsRangedDestructive.Clear();
		pawnsMeleeDestructive.Clear();
		pawnsRangedGeneral.Clear();
		pawnSoloAttackers.Clear();
		pawnsEscort.Clear();
		pawnsLost.Clear();
		Data.maxRange = 12f;
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			if (!pawn.CanReach(Data.breachStart, PathEndMode.OnCell, Danger.Deadly))
			{
				pawnsLost.Add(pawn);
				continue;
			}
			Verb verb = BreachingUtility.FindVerbToUseForBreaching(pawn);
			if (verb == null)
			{
				pawnsEscort.Add(pawn);
			}
			else if (!pawn.RaceProps.IsMechanoid && BreachingUtility.IsSoloAttackVerb(verb))
			{
				pawnSoloAttackers.Add(pawn);
			}
			else if (verb.verbProps.ai_IsBuildingDestroyer)
			{
				if (verb.IsMeleeAttack)
				{
					pawnsMeleeDestructive.Add(pawn);
					continue;
				}
				pawnsRangedDestructive.Add(pawn);
				Data.maxRange = Math.Min(Data.maxRange, verb.EffectiveRange);
			}
			else if (verb.IsMeleeAttack)
			{
				pawnsEscort.Add(pawn);
			}
			else
			{
				pawnsRangedGeneral.Add(pawn);
			}
		}
		bool num = pawnsMeleeDestructive.Any();
		bool flag = pawnsRangedDestructive.Any();
		if (num && (!flag || Data.preferMelee))
		{
			BalanceAndSetDuties(Data.breachDest, pawnsMeleeDestructive, pawnSoloAttackers, pawnsRangedDestructive, pawnsRangedGeneral, pawnsEscort);
			SetBackupDuty(pawnsLost);
			return;
		}
		if (flag)
		{
			BalanceAndSetDuties(Data.breachDest, pawnsRangedDestructive, pawnSoloAttackers, pawnsRangedGeneral, pawnsMeleeDestructive, pawnsEscort);
			SetBackupDuty(pawnsLost);
			return;
		}
		if (pawnsRangedGeneral.Any())
		{
			BalanceAndSetDuties(Data.breachDest, pawnsRangedGeneral, pawnSoloAttackers, pawnsMeleeDestructive, pawnsRangedDestructive, pawnsEscort);
			SetBackupDuty(pawnsLost);
			return;
		}
		SetBackupDuty(pawnsMeleeDestructive);
		SetBackupDuty(pawnsRangedDestructive);
		SetBackupDuty(pawnsRangedGeneral);
		SetBackupDuty(pawnSoloAttackers);
		SetBackupDuty(pawnsEscort);
		SetBackupDuty(pawnsLost);
	}

	private static void BalanceAndSetDuties(IntVec3 breachDest, List<Pawn> breachers, List<Pawn> soloAttackers, List<Pawn> escorts1, List<Pawn> escorts2, List<Pawn> escorts3)
	{
		if (!escorts1.Any() && !escorts2.Any() && !escorts3.Any())
		{
			if (soloAttackers.Any())
			{
				escorts3.AddRange(soloAttackers);
				soloAttackers.Clear();
			}
			else if (breachers.Count > 1)
			{
				Pawn item = breachers.First();
				breachers.Remove(item);
				escorts3.Add(item);
			}
		}
		SetBreachDuty(breachers, breachDest);
		SetSoloAttackDuty(soloAttackers, breachDest);
		SetEscortDuty(escorts1, breachers);
		SetEscortDuty(escorts2, breachers);
		SetEscortDuty(escorts3, breachers);
	}

	private static void SetBackupDuty(List<Pawn> pawns)
	{
		for (int i = 0; i < pawns.Count; i++)
		{
			pawns[i].mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
		}
	}

	private static void SetEscortDuty(List<Pawn> escorts, List<Pawn> targets)
	{
		for (int i = 0; i < escorts.Count; i++)
		{
			Pawn pawn = escorts[i];
			pawn.mindState.duty = new PawnDuty(DutyDefOf.Escort, targets.RandomElement(), BreachingUtility.EscortRadius(pawn));
		}
	}

	private static void SetBreachDuty(List<Pawn> breachers, IntVec3 breachDest)
	{
		for (int i = 0; i < breachers.Count; i++)
		{
			breachers[i].mindState.duty = new PawnDuty(DutyDefOf.Breaching, breachDest);
		}
	}

	private static void SetSoloAttackDuty(List<Pawn> breachers, IntVec3 breachDest)
	{
		for (int i = 0; i < breachers.Count; i++)
		{
			breachers[i].mindState.duty = new PawnDuty(DutyDefOf.Breaching, breachDest);
		}
	}

	public void UpdateCurrentBreachTarget()
	{
		if (Data.currentTarget != null && Data.currentTarget.Destroyed)
		{
			Data.currentTarget = null;
		}
		if (Data.soloAttacker != null && BreachingUtility.FindVerbToUseForBreaching(Data.soloAttacker) == null)
		{
			Data.currentTarget = null;
		}
		if (Data.currentTarget != null)
		{
			return;
		}
		Data.currentTarget = Data.breachingGrid.FindBuildingToBreach();
		Data.soloAttacker = null;
		if (pawnSoloAttackers.Any() && BreachingUtility.CanSoloAttackTargetBuilding(Data.currentTarget) && Rand.Chance(0.2f))
		{
			Pawn pawn = pawnSoloAttackers.RandomElement();
			if (BreachingUtility.FindVerbToUseForBreaching(pawn) != null)
			{
				Data.soloAttacker = pawn;
			}
		}
	}
}
