using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_RevenantEscape : JobDriver
{
	private static int MinEscapeTime = 300;

	private static int EscapedCheckInterval = 120;

	private static int SmearMTBTicks = 60;

	private static readonly int SmearLeavingDuration = Mathf.FloorToInt((float)MinEscapeTime * 1.4f);

	private CompRevenant Comp => pawn.TryGetComp<CompRevenant>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Comp.becomeInvisibleTick = Find.TickManager.TicksGame + 140;
		Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			if (Rand.MTBEventOccurs(SmearMTBTicks, 1f, delta))
			{
				RevenantUtility.CreateRevenantSmear(pawn);
			}
			if (Rand.MTBEventOccurs(EscapedCheckInterval, 1f, delta) && Find.TickManager.TicksGame > pawn.mindState.lastEngageTargetTick + MinEscapeTime && RevenantUtility.NearbyHumanlikePawnCount(pawn.Position, pawn.Map, 20f) == 0)
			{
				ReadyForNextToil();
			}
		});
		yield return toil;
		Toil toil2 = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		toil2.initAction = (Action)Delegate.Combine(toil2.initAction, (Action)delegate
		{
			Comp.escapeSecondStageStartedTick = Find.TickManager.TicksGame;
		});
		toil2.tickIntervalAction = (Action<int>)Delegate.Combine(toil2.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			if (Find.AnalysisManager.TryGetAnalysisProgress(Comp.biosignature, out var details) && (details.Satisfied || details.timesDone >= 1) && Find.TickManager.TicksGame < Comp.escapeSecondStageStartedTick + SmearLeavingDuration && Rand.MTBEventOccurs(SmearMTBTicks, 1f, delta))
			{
				RevenantUtility.CreateRevenantSmear(pawn);
			}
			if (Find.TickManager.TicksGame > Comp.escapeSecondStageStartedTick + MinEscapeTime * 2 && RevenantUtility.NearbyHumanlikePawnCount(pawn.Position, pawn.Map, 20f) == 0)
			{
				pawn.TryGetComp<CompRevenant>().revenantState = RevenantState.Sleep;
				EndJobWith(JobCondition.Succeeded);
			}
		});
		yield return toil2;
	}
}
