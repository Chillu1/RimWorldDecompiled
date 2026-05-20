using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public class Verb_LaunchProjectileStaticOneUse : Verb_LaunchProjectileStatic
{
	protected override bool TryCastShot()
	{
		if (base.TryCastShot())
		{
			if (burstShotsLeft <= 1)
			{
				SelfConsume();
			}
			return true;
		}
		if (burstShotsLeft < base.BurstShotCount)
		{
			SelfConsume();
		}
		return false;
	}

	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		if (!base.ValidateTarget(target, showMessages: true))
		{
			return false;
		}
		if (target.Cell.GetFirstBuilding(Find.CurrentMap) == null)
		{
			return target.Cell.Standable(Find.CurrentMap);
		}
		return false;
	}

	public override void OnGUI(LocalTargetInfo target)
	{
		Map currentMap = Find.CurrentMap;
		base.OnGUI(target);
		Texture2D icon = (target.Cell.InBounds(currentMap) ? (ValidateTarget(target, showMessages: false) ? TexCommand.Attack : TexCommand.CannotShoot) : TexCommand.CannotShoot);
		GenUI.DrawMouseAttachment(icon);
	}

	public override void Notify_EquipmentLost()
	{
		base.Notify_EquipmentLost();
		if (state == VerbState.Bursting && burstShotsLeft < base.BurstShotCount)
		{
			SelfConsume();
		}
	}

	private void SelfConsume()
	{
		if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
		{
			base.EquipmentSource.Destroy();
		}
	}

	public override void OrderForceTarget(LocalTargetInfo target)
	{
		Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStaticReserve, target);
		job.verbToUse = this;
		CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}
}
