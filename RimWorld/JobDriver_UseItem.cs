using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_UseItem : JobDriver
{
	private int useDuration = -1;

	private Mote warmupMote;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref useDuration, "useDuration", 0);
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		useDuration = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable>().Props.useDuration;
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (job.targetB.IsValid && !pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
		this.FailOn(() => !base.TargetThingA.TryGetComp<CompUsable>().CanBeUsedBy(pawn));
		yield return Toils_Goto.GotoThing(TargetIndex.A, base.TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch);
		yield return PrepareToUse();
		yield return Use();
	}

	protected Toil PrepareToUse()
	{
		Toil toil = Toils_General.Wait(useDuration, TargetIndex.A);
		toil.WithProgressBarToilDelay(TargetIndex.A);
		toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		toil.FailOnCannotTouch(TargetIndex.A, base.TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch);
		toil.handlingFacing = true;
		toil.tickAction = delegate
		{
			if (job.GetTarget(TargetIndex.A).Thing is ThingWithComps thingWithComps)
			{
				foreach (CompUseEffect comp in thingWithComps.GetComps<CompUseEffect>())
				{
					comp?.PrepareTick();
				}
			}
			else
			{
				job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUseEffect>()?.PrepareTick();
			}
			CompUsable compUsable = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable>();
			if (compUsable != null && warmupMote == null && compUsable.Props.warmupMote != null)
			{
				warmupMote = MoteMaker.MakeAttachedOverlay(job.GetTarget(TargetIndex.B).Thing, compUsable.Props.warmupMote, Vector3.zero);
			}
			warmupMote?.Maintain();
			pawn.rotationTracker.FaceTarget(base.TargetA);
		};
		if (job.targetB.IsValid)
		{
			toil.FailOnDespawnedOrNull(TargetIndex.B);
			CompTargetable compTargetable = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompTargetable>();
			if (compTargetable != null && compTargetable.Props.nonDownedPawnOnly)
			{
				toil.FailOnDestroyedOrNull(TargetIndex.B);
				toil.FailOnDowned(TargetIndex.B);
			}
		}
		return toil;
	}

	protected Toil Use()
	{
		Toil use = ToilMaker.MakeToil("Use");
		use.initAction = delegate
		{
			Pawn actor = use.actor;
			CompUsable compUsable = actor.CurJob.targetA.Thing.TryGetComp<CompUsable>();
			compUsable.UsedBy(actor);
			if (compUsable.Props.finalizeMote != null)
			{
				MoteMaker.MakeAttachedOverlay(actor.CurJob.GetTarget(TargetIndex.A).Thing, compUsable.Props.finalizeMote, Vector3.zero);
			}
		};
		use.defaultCompleteMode = ToilCompleteMode.Instant;
		return use;
	}
}
