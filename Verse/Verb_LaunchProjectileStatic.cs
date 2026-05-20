using RimWorld;
using RimWorld.Utility;
using UnityEngine;
using Verse.AI;

namespace Verse;

public class Verb_LaunchProjectileStatic : Verb_LaunchProjectile
{
	public override bool MultiSelect => true;

	public override Texture2D UIIcon => TexCommand.Attack;

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
}
