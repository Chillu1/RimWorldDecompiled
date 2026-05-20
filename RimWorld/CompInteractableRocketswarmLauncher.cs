using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class CompInteractableRocketswarmLauncher : CompInteractable
{
	private Building_TurretGun ParentGun => (Building_TurretGun)parent;

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		if (ValidateTarget(target, showMessages: false))
		{
			TargetLocation(target.Pawn);
		}
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if ((bool)CanInteract(selPawn))
		{
			yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(base.Props.jobString.CapitalizeFirst(), delegate
			{
				TargetLocation(selPawn);
			}), selPawn, parent);
		}
	}

	private void TargetLocation(Pawn caster)
	{
		Find.Targeter.BeginTargeting(ParentGun.AttackVerb, null, allowNonSelectedTargetingSource: true, null, delegate
		{
			if (ParentGun.ForcedTarget.IsValid)
			{
				Job job = JobMaker.MakeJob(JobDefOf.InteractThing, parent);
				caster.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		});
	}

	protected override bool TryInteractTick()
	{
		ParentGun.TryActivateBurst();
		if (ParentGun.CurrentTarget.IsValid)
		{
			ParentGun.Top.ForceFaceTarget(ParentGun.CurrentTarget);
			return true;
		}
		return true;
	}

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		AcceptanceReport result = base.CanInteract(activateBy, checkOptionalItems);
		if (!result.Accepted)
		{
			return result;
		}
		if (activateBy != null && activateBy.WorkTagIsDisabled(WorkTags.Violent))
		{
			return "IsIncapableOfViolence".Translate(activateBy.LabelShort, activateBy);
		}
		return true;
	}
}
