using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class JobDriver_HaulToContainer : JobDriver
	{
		protected const TargetIndex CarryThingIndex = TargetIndex.A;

		public const TargetIndex DestIndex = TargetIndex.B;

		protected const TargetIndex PrimaryDestIndex = TargetIndex.C;

		public Thing ThingToCarry => (Thing)job.GetTarget(TargetIndex.A);

		public Thing Container => (Thing)job.GetTarget(TargetIndex.B);

		private int Duration
		{
			get
			{
				if (Container == null || !(Container is Building))
				{
					return 0;
				}
				return Container.def.building.haulToContainerDuration;
			}
		}

		public override string GetReport()
		{
			Thing thing = null;
			thing = ((pawn.CurJob != job || pawn.carryTracker.CarriedThing == null) ? base.TargetThingA : pawn.carryTracker.CarriedThing);
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
			if (!pawn.Reserve(job.GetTarget(TargetIndex.B), job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
			this.FailOn(() => TransporterUtility.WasLoadingCanceled(Container));
			this.FailOn(delegate
			{
				ThingOwner thingOwner = Container.TryGetInnerInteractableThingOwner();
				if (thingOwner != null && !thingOwner.CanAcceptAnyOf(ThingToCarry))
				{
					return true;
				}
				IHaulDestination haulDestination = Container as IHaulDestination;
				return (haulDestination != null && !haulDestination.Accepts(ThingToCarry)) ? true : false;
			});
			Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return getToHaulTarget;
			yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
			yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(getToHaulTarget, TargetIndex.A);
			Toil carryToContainer = Toils_Haul.CarryHauledThingToContainer();
			yield return carryToContainer;
			yield return Toils_Goto.MoveOffTargetBlueprint(TargetIndex.B);
			Toil toil = Toils_General.Wait(Duration, TargetIndex.B);
			toil.WithProgressBarToilDelay(TargetIndex.B);
			yield return toil;
			yield return Toils_Construct.MakeSolidThingFromBlueprintIfNecessary(TargetIndex.B, TargetIndex.C);
			yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.C);
			yield return Toils_Haul.JumpToCarryToNextContainerIfPossible(carryToContainer, TargetIndex.C);
		}
	}
}
