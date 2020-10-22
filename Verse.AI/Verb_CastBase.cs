using RimWorld;
using UnityEngine;

namespace Verse.AI
{
	public abstract class Verb_CastBase : Verb
	{
		public override bool MultiSelect => true;

		public override bool ValidateTarget(LocalTargetInfo target)
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
			CasterPawn.jobs.TryTakeOrderedJob(job);
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
			if (verbProps.requireLineOfSight)
			{
				GenDraw.DrawRadiusRing(caster.Position, EffectiveRange, Color.white, CanTarget);
			}
			else
			{
				GenDraw.DrawRadiusRing(caster.Position, EffectiveRange);
			}
			bool CanTarget(IntVec3 c)
			{
				return GenSight.LineOfSight(caster.Position, c, caster.Map);
			}
		}
	}
}
