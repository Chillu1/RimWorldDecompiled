using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_GetReimplanted : JobDriver
{
	public Pawn Target => job.targetA.Thing as Pawn;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModLister.CheckBiotech("xenogerm reimplanting"))
		{
			yield break;
		}
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnDowned(TargetIndex.A);
		this.FailOn(() => pawn.genes == null || Target.genes == null || GeneUtility.SameXenotype(Target, pawn));
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_General.Do(delegate
		{
			Ability ability = Target.abilities.abilities.FirstOrFallback((Ability x) => x.def == AbilityDefOf.ReimplantXenogerm);
			if (ability != null)
			{
				Target.jobs.TryTakeOrderedJob(ability.GetJob(pawn, pawn), JobTag.Misc);
			}
		});
	}
}
