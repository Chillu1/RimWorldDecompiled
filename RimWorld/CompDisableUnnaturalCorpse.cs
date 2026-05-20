using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompDisableUnnaturalCorpse : CompInteractable
{
	public UnnaturalCorpse Corpse => (UnnaturalCorpse)parent;

	public new CompProperties_DisableUnnaturalCorpse Props => (CompProperties_DisableUnnaturalCorpse)props;

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		OrderActivation(target.Pawn);
	}

	protected override void OnInteracted(Pawn caster)
	{
		Corpse.DoStudiedDeactivation(caster);
	}

	public override string CompInspectStringExtra()
	{
		return null;
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (Corpse.Tracker.CanDestroyViaResearch)
		{
			AcceptanceReport acceptanceReport = CanInteract(selPawn);
			FloatMenuOption floatMenuOption = new FloatMenuOption(Props.jobString.CapitalizeFirst(), delegate
			{
				OrderActivation(selPawn);
			});
			if (!acceptanceReport.Accepted)
			{
				floatMenuOption.Disabled = true;
				floatMenuOption.Label = floatMenuOption.Label + " (" + acceptanceReport.Reason + ")";
			}
			yield return floatMenuOption;
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (!Corpse.Tracker.CanDestroyViaResearch)
		{
			yield break;
		}
		foreach (Gizmo item in base.CompGetGizmosExtra())
		{
			yield return item;
		}
	}

	private void OrderActivation(Pawn pawn)
	{
		Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent);
		job.count = 1;
		job.playerForced = true;
		pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}
}
