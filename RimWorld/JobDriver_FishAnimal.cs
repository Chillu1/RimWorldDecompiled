using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_FishAnimal : JobDriver
{
	private const TargetIndex SpotInd = TargetIndex.A;

	private const TargetIndex StandInd = TargetIndex.B;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
		int ticks = Mathf.RoundToInt(1500f / pawn.GetStatValue(StatDefOf.FishingSpeed));
		Toil toil = Toils_General.WaitWith(TargetIndex.A, ticks, useProgressBar: false, maintainPosture: true, maintainSleep: false, TargetIndex.A);
		toil.WithProgressBarToilDelay(TargetIndex.B);
		yield return toil;
		yield return CompleteFishingToil();
	}

	private Toil CompleteFishingToil()
	{
		Toil toil = ToilMaker.MakeToil("CompleteFishingToil");
		toil.initAction = delegate
		{
			IntVec3 cell = job.GetTarget(TargetIndex.A).Cell;
			bool rare;
			List<Thing> catchesFor = FishingUtility.GetCatchesFor(pawn, cell, animalFishing: true, out rare);
			if (catchesFor.Any())
			{
				bool flag = false;
				int num = catchesFor.Sum((Thing x) => x.stackCount);
				foreach (Thing item in catchesFor)
				{
					flag |= GenPlace.TryPlaceThing(item, pawn.Position, pawn.Map, ThingPlaceMode.Near);
				}
				if (flag)
				{
					pawn.Map.waterBodyTracker.Notify_Fished(cell, num);
				}
			}
		};
		return toil;
	}
}
