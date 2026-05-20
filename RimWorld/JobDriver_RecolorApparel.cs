using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_RecolorApparel : JobDriver
{
	public const TargetIndex DyeInd = TargetIndex.A;

	public const TargetIndex ApparelInd = TargetIndex.B;

	public const TargetIndex StylingStationInd = TargetIndex.C;

	public const int WorkTimeTicks = 1000;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(job.GetTarget(TargetIndex.C), job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		Thing thing = job.GetTarget(TargetIndex.C).Thing;
		if (thing != null && thing.def.hasInteractionCell && !pawn.ReserveSittableOrSpot(thing.InteractionCell, job, errorOnFailed))
		{
			return false;
		}
		int num = job.GetTargetQueue(TargetIndex.B).Count;
		foreach (LocalTargetInfo item in job.GetTargetQueue(TargetIndex.A))
		{
			int num2 = Mathf.Min(num, item.Thing.stackCount);
			if (!pawn.Reserve(item, job, 1, num2, null, errorOnFailed))
			{
				return false;
			}
			num -= num2;
			if (num2 <= 0)
			{
				break;
			}
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckIdeology("Apparel recoloring"))
		{
			yield break;
		}
		this.FailOnDespawnedOrNull(TargetIndex.C);
		foreach (Toil item in JobDriver_DoBill.CollectIngredientsToils(TargetIndex.A, TargetIndex.C, TargetIndex.B, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: false))
		{
			yield return item;
		}
		yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.InteractionCell);
		Toil work = Toils_General.WaitWith(TargetIndex.C, 1000, useProgressBar: true, maintainPosture: false, maintainSleep: false, TargetIndex.C);
		work.PlaySustainerOrSound(SoundDefOf.Interact_RecolorApparel);
		yield return work;
		yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
		yield return Toils_General.Do(delegate
		{
			ThingCountClass thingCountClass = job.placedThings[0];
			if (thingCountClass.thing.stackCount == 1)
			{
				thingCountClass.thing.Destroy();
				job.placedThings.RemoveAt(0);
			}
			else if (!thingCountClass.thing.Destroyed)
			{
				thingCountClass.thing.SplitOff(1).Destroy();
			}
			job.GetTarget(TargetIndex.B).Thing.TryGetComp<CompColorable>().Recolor();
			job.count = job.GetTargetQueue(TargetIndex.B)?.Count ?? 0;
		});
		yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, work);
	}
}
