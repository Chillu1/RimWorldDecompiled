using RimWorld;
using RimWorld.Utility;
using UnityEngine;

namespace Verse.AI;

public abstract class Verb_CastBase : Verb
{
	public override bool MultiSelect => true;

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!base.ValidateTarget(target))
		{
			return false;
		}
		if (!ReloadableUtility.CanUseConsideringQueuedJobs(CasterPawn, base.EquipmentSource))
		{
			return false;
		}
		return true;
	}

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, target);
		job.verbToUse = this;
		CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}

	public override void OnGUI(LocalTargetInfo target)
	{
		if (CanHitTarget(target) && verbProps.targetParams.CanTarget(target.ToTargetInfo(caster.Map)))
		{
			base.OnGUI(target);
		}
		else
		{
			GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
		}
	}

	public override void DrawHighlight(LocalTargetInfo target)
	{
		if (target.IsValid && CanHitTarget(target))
		{
			GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
			DrawHighlightFieldRadiusAroundTarget(target);
		}
		GenDraw.DrawRadiusRing(caster.Position, EffectiveRange, Color.white, (IntVec3 c) => CanHitTarget(c));
	}
}
