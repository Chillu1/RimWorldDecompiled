using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI;

public class JobDriver_HaulToContainer : JobDriver, IBuildableDriver
{
	private Effecter graveDigEffect;

	protected const TargetIndex CarryThingIndex = TargetIndex.A;

	public const TargetIndex DestIndex = TargetIndex.B;

	protected const TargetIndex PrimaryDestIndex = TargetIndex.C;

	protected const int DiggingEffectInterval = 80;

	public Thing ThingToCarry => (Thing)job.GetTarget(TargetIndex.A);

	public Thing Container => (Thing)job.GetTarget(TargetIndex.B);

	public ThingDef ThingDef => ThingToCarry.def;

	protected virtual int Duration
	{
		get
		{
			if (Container == null || !(Container is Building building))
			{
				return 0;
			}
			return building.HaulToContainerDuration(ThingToCarry);
		}
	}

	protected virtual EffecterDef WorkEffecter => null;

	protected virtual SoundDef WorkSustainer => null;

	public bool TryGetBuildableRect(out CellRect rect)
	{
		if (Container is Blueprint)
		{
			rect = Container.OccupiedRect();
			return true;
		}
		rect = default(CellRect);
		return false;
	}

	public override string GetReport()
	{
		Thing thing = ((pawn.CurJob != job || pawn.carryTracker.CarriedThing == null) ? base.TargetThingA : pawn.carryTracker.CarriedThing);
		if (thing == null || !job.targetB.HasThing)
		{
			return "ReportHaulingUnknown".Translate();
		}
		return ((job.GetTarget(TargetIndex.B).Thing is Building_Grave) ? "ReportHaulingToGrave" : "ReportHaulingTo").Translate(thing.Label, job.targetB.Thing.LabelShort.Named("DESTINATION"), thing.Named("THING"));
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		if (Container.Isnt<IHaulEnroute>())
		{
			if (!pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, 1, null, errorOnFailed))
			{
				return false;
			}
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
		}
		UpdateEnrouteTrackers();
		pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
		return true;
	}

	protected virtual void ModifyPrepareToil(Toil toil)
	{
	}

	private bool TryReplaceWithFrame(TargetIndex index)
	{
		Thing thing = GetActor().jobs.curJob.GetTarget(index).Thing;
		Building edifice = thing.Position.GetEdifice(pawn.Map);
		if (edifice != null && thing is Blueprint_Build blueprint_Build && edifice is Frame frame && frame.BuildDef == blueprint_Build.BuildDef)
		{
			job.SetTarget(TargetIndex.B, frame);
			return true;
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOn(delegate
		{
			Thing thing = GetActor().jobs.curJob.GetTarget(TargetIndex.B).Thing;
			Thing thing2 = GetActor().jobs.curJob.GetTarget(TargetIndex.C).Thing;
			if (thing == null)
			{
				return true;
			}
			if (thing2 != null && thing2.Destroyed && !TryReplaceWithFrame(TargetIndex.C))
			{
				job.SetTarget(TargetIndex.C, null);
			}
			if (!thing.Spawned || (thing.Destroyed && !TryReplaceWithFrame(TargetIndex.B)))
			{
				if (job.targetQueueB.NullOrEmpty())
				{
					return true;
				}
				if (!Toils_Haul.TryGetNextDestinationFromQueue(TargetIndex.C, TargetIndex.B, ThingDef, job, pawn, out var nextTarget))
				{
					return true;
				}
				job.targetQueueB.RemoveAll((LocalTargetInfo target) => target.Thing == nextTarget);
				job.targetB = nextTarget;
			}
			ThingOwner thingOwner = Container.TryGetInnerInteractableThingOwner();
			if (thingOwner != null && !thingOwner.CanAcceptAnyOf(ThingToCarry))
			{
				return true;
			}
			return (Container is IHaulDestination haulDestination && !haulDestination.Accepts(ThingToCarry)) ? true : false;
		});
		this.FailOnForbidden(TargetIndex.B);
		this.FailOn(() => EnterPortalUtility.WasLoadingCanceled(Container));
		this.FailOn(() => TransporterUtility.WasLoadingCanceled(Container));
		this.FailOn(() => CompBiosculpterPod.WasLoadingCanceled(Container));
		this.FailOn(() => Building_SubcoreScanner.WasLoadingCancelled(Container));
		Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, canGotoSpawnedParent: true).FailOn(() => ThingToCarry.ParentHolder is MinifiedThing).FailOnSelfAndParentsDespawnedOrNull(TargetIndex.A);
		Toil uninstallIfMinifiable = Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A).FailOn(() => ThingToCarry.ParentHolder is MinifiedThing)
			.FailOnSelfAndParentsDespawnedOrNull(TargetIndex.A)
			.FailOnDestroyedOrNull(TargetIndex.A);
		Toil startCarryingThing = Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true, failIfStackCountLessThanJobCount: false, reserve: true, canTakeFromInventory: true);
		Toil jumpIfAlsoCollectingNextTarget = Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.A);
		Toil carryToContainer = ((base.TargetThingB is Blueprint || base.TargetThingB is Frame) ? Toils_Goto.GotoBuild(TargetIndex.B) : Toils_Haul.CarryHauledThingToContainer());
		yield return Toils_Jump.JumpIf(jumpIfAlsoCollectingNextTarget, () => pawn.IsCarryingThing(ThingToCarry));
		yield return getToHaulTarget;
		yield return uninstallIfMinifiable;
		yield return startCarryingThing;
		yield return jumpIfAlsoCollectingNextTarget;
		yield return carryToContainer;
		yield return Toils_Goto.MoveOffTargetBlueprint(TargetIndex.B);
		Toil toil = Toils_General.Wait(Duration, TargetIndex.B);
		toil.WithProgressBarToilDelay(TargetIndex.B);
		EffecterDef workEffecter = WorkEffecter;
		if (workEffecter != null)
		{
			toil.WithEffect(workEffecter, TargetIndex.B);
		}
		SoundDef workSustainer = WorkSustainer;
		if (workSustainer != null)
		{
			toil.PlaySustainerOrSound(workSustainer);
		}
		Thing destThing = job.GetTarget(TargetIndex.B).Thing;
		toil.tickIntervalAction = delegate(int delta)
		{
			if (pawn.IsHashIntervalTick(80, delta) && destThing is Building_Grave && graveDigEffect == null)
			{
				graveDigEffect = EffecterDefOf.BuryPawn.Spawn();
				graveDigEffect.Trigger(destThing, destThing);
			}
		};
		toil.tickAction = delegate
		{
			graveDigEffect?.EffectTick(destThing, destThing);
		};
		ModifyPrepareToil(toil);
		yield return toil;
		yield return Toils_Construct.MakeSolidThingFromBlueprintIfNecessary(TargetIndex.B, TargetIndex.C);
		yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.C);
		yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer, TargetIndex.C);
	}

	private void UpdateEnrouteTrackers()
	{
		int count = job.count;
		TryReserveEnroute(base.TargetThingC, ref count);
		if (base.TargetB != base.TargetC)
		{
			TryReserveEnroute(base.TargetThingB, ref count);
		}
		if (job.targetQueueB == null)
		{
			return;
		}
		foreach (LocalTargetInfo item in job.targetQueueB)
		{
			if (!base.TargetC.HasThing || !(item == base.TargetThingC))
			{
				TryReserveEnroute(item.Thing, ref count);
			}
		}
	}

	private void TryReserveEnroute(Thing thing, ref int count)
	{
		if (thing is IHaulEnroute container && !thing.DestroyedOrNull())
		{
			UpdateTracker(container, ref count);
		}
	}

	private void UpdateTracker(IHaulEnroute container, ref int count)
	{
		if (!ThingToCarry.DestroyedOrNull())
		{
			if (job.playerForced && container.GetSpaceRemainingWithEnroute(ThingDef) == 0)
			{
				container.Map.enrouteManager.InterruptEnroutePawns(container, pawn);
			}
			int num = Mathf.Min(count, container.GetSpaceRemainingWithEnroute(ThingDef));
			if (num > 0)
			{
				container.Map.enrouteManager.AddEnroute(container, pawn, base.TargetThingA.def, num);
			}
			count -= num;
		}
	}
}
