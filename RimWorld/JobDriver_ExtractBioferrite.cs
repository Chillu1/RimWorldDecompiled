using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ExtractBioferrite : JobDriver
{
	private const TargetIndex PlatformIndex = TargetIndex.A;

	private const int ExtractDuration = 2000;

	private const int CutDamage = 4;

	private const int BioferriteModifier = 4;

	private Thing Platform => base.TargetThingA;

	private Pawn InnerPawn => (Platform as Building_HoldingPlatform)?.HeldPawn;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(Platform, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(delegate
		{
			Pawn innerPawn = InnerPawn;
			if (innerPawn == null || innerPawn.Destroyed)
			{
				return true;
			}
			CompHoldingPlatformTarget compHoldingPlatformTarget = innerPawn.TryGetComp<CompHoldingPlatformTarget>();
			return (compHoldingPlatformTarget != null && (compHoldingPlatformTarget.containmentMode == EntityContainmentMode.Release || !compHoldingPlatformTarget.extractBioferrite)) ? true : false;
		});
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		int ticks = (int)(1f / pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * 2000f);
		Toil toil = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: true, maintainPosture: false, maintainSleep: false, TargetIndex.A);
		toil.activeSkill = () => SkillDefOf.Medicine;
		toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch).WithProgressBarToilDelay(TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Recipe_Surgery);
		yield return toil;
		yield return Toils_General.Do(delegate
		{
			Pawn innerPawn = InnerPawn;
			int num = Mathf.FloorToInt(CompProducesBioferrite.BioferritePerDay(innerPawn) * 4f);
			if (num > 0)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Bioferrite);
				thing.stackCount = num;
				GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
			}
			innerPawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 4f));
			innerPawn.health.AddHediff(HediffDefOf.BioferriteExtracted);
		});
	}

	public override string GetReport()
	{
		return JobUtility.GetResolvedJobReport(job.def.reportString, InnerPawn, job.targetB, job.targetC);
	}
}
