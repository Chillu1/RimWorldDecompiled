using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Deathrest : JobDriver
{
	private const TargetIndex BedIndex = TargetIndex.A;

	private const int MoteIntervalTicks = 160;

	private Building_Bed Bed => job.GetTarget(TargetIndex.A).Thing as Building_Bed;

	public override bool PlayerInterruptable => !base.OnLastToil;

	public override string GetReport()
	{
		return ReportStringProcessed(SanguophageUtility.DeathrestJobReport(pawn));
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (Bed != null)
		{
			return pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed);
		}
		return true;
	}

	public override bool CanBeginNowWhileLyingDown()
	{
		return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(TargetIndex.A));
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		bool hasBed = Bed != null;
		if (hasBed)
		{
			yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
			Toil toil = Toils_Bed.GotoBed(TargetIndex.A);
			toil.AddFailCondition(delegate
			{
				if (SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead(pawn))
				{
					return false;
				}
				foreach (IntVec3 item in Bed.OccupiedRect())
				{
					if (item.InSunlight(Bed.Map))
					{
						Messages.Message("MessageBedExposedToSunlight".Translate(pawn.Named("PAWN"), Bed.Named("BED")), Bed, MessageTypeDefOf.RejectInput);
						return true;
					}
				}
				return false;
			});
			yield return toil;
		}
		else
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		}
		Toil toil2 = Toils_LayDown.LayDown(TargetIndex.A, hasBed, lookForOtherJobs: false, canSleep: true, gainRestAndHealth: true, PawnPosture.LayingOnGroundNormal, deathrest: true);
		toil2.initAction = (Action)Delegate.Combine(toil2.initAction, (Action)delegate
		{
			if (pawn.Drafted)
			{
				pawn.drafter.Drafted = false;
			}
			if (!pawn.health.hediffSet.HasHediff(HediffDefOf.Deathrest))
			{
				pawn.health.AddHediff(HediffDefOf.Deathrest);
			}
		});
		toil2.tickIntervalAction = (Action<int>)Delegate.Combine(toil2.tickIntervalAction, (Action<int>)delegate(int delta)
		{
			if (pawn.IsHashIntervalTick(160, delta))
			{
				MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_Deathresting, new Vector3(0f, pawn.story.bodyType.bedOffset).RotatedBy(pawn.Rotation));
			}
		});
		yield return toil2;
	}
}
