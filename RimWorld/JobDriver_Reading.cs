using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Reading : JobDriver
{
	private bool hasInInventory;

	private bool carrying;

	private bool isLearningDesire;

	private bool isReading;

	public const TargetIndex BookIndex = TargetIndex.A;

	public const TargetIndex SurfaceIndex = TargetIndex.B;

	private const int ManualReadTicks = 5000;

	private const int ChairSearchRadius = 32;

	private const int UrgentJobCheckIntervalTicks = 600;

	public Book Book => job.GetTarget(TargetIndex.A).Thing as Book;

	public bool IsReading => isReading;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Book, job, 1, 1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		SetFinalizerJob(delegate(JobCondition condition)
		{
			if (!pawn.IsCarryingThing(Book))
			{
				return (Job)null;
			}
			if (condition != JobCondition.Succeeded)
			{
				pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Direct, out var _);
				return (Job)null;
			}
			return HaulAIUtility.HaulToStorageJob(pawn, Book, forced: false);
		});
		foreach (Toil item in PrepareToReadBook())
		{
			yield return item;
		}
		int duration = (job.playerForced ? 5000 : job.def.joyDuration);
		yield return ReadBook(duration);
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		job.count = 1;
		hasInInventory = pawn.inventory != null && pawn.inventory.Contains(Book);
		carrying = pawn?.carryTracker.CarriedThing == Book;
		isLearningDesire = pawn?.learning != null && pawn.learning.ActiveLearningDesires.Contains(LearningDesireDefOf.Reading);
	}

	private IEnumerable<Toil> PrepareToReadBook()
	{
		if (!carrying)
		{
			if (hasInInventory)
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
			}
			else
			{
				yield return Toils_Goto.GotoCell(Book.PositionHeld, PathEndMode.ClosestTouch).FailOnDestroyedOrNull(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
				yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: false, reserve: true, canTakeFromInventory: true);
			}
			yield return CarryToReadingSpot().FailOnDestroyedOrNull(TargetIndex.A);
			yield return FindAdjacentReadingSurface();
		}
	}

	private Toil ReadBook(int duration)
	{
		Toil toil = Toils_General.Wait(duration);
		toil.debugName = "Reading";
		toil.FailOnDestroyedNullOrForbidden(TargetIndex.A);
		toil.handlingFacing = true;
		toil.initAction = delegate
		{
			Book.IsOpen = true;
			pawn.pather.StopDead();
			job.showCarryingInspectLine = false;
		};
		toil.tickIntervalAction = delegate(int delta)
		{
			if (job.GetTarget(TargetIndex.B).IsValid)
			{
				pawn.rotationTracker.FaceCell(job.GetTarget(TargetIndex.B).Cell);
			}
			else if (Book.Spawned)
			{
				pawn.rotationTracker.FaceCell(Book.Position);
			}
			else if (pawn.Rotation == Rot4.North)
			{
				pawn.Rotation = new Rot4(Rand.Range(1, 4));
			}
			float readingBonus = BookUtility.GetReadingBonus(pawn);
			isReading = true;
			Book.OnBookReadTick(pawn, delta, readingBonus);
			pawn.skills?.Learn(SkillDefOf.Intellectual, 0.1f * (float)delta);
			pawn.GainComfortFromCellIfPossible(delta);
			if (pawn.CurJob != null && pawn.needs?.joy != null)
			{
				JoyTickFullJoyAction fullJoyAction = JoyTickFullJoyAction.GoToNextToil;
				if (pawn.CurJob.playerForced || pawn.learning != null)
				{
					fullJoyAction = JoyTickFullJoyAction.None;
				}
				JoyUtility.JoyTickCheckEnd(pawn, delta, fullJoyAction, Book.JoyFactor * readingBonus);
			}
			if (isLearningDesire && job != null)
			{
				if (pawn.needs?.learning != null)
				{
					LearningUtility.LearningTickCheckEnd(pawn, delta, job.playerForced);
				}
				else
				{
					pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
				}
			}
			if (pawn.IsHashIntervalTick(600, delta))
			{
				pawn.jobs.CheckForJobOverride(9.1f);
			}
		};
		toil.AddEndCondition(() => BookUtility.CanReadBook(Book, pawn, out var _) ? JobCondition.Ongoing : JobCondition.InterruptForced);
		toil.AddFinishAction(delegate
		{
			Book.IsOpen = false;
			TaleRecorder.RecordTale(TaleDefOf.ReadBook, pawn, Book);
			JoyUtility.TryGainRecRoomThought(pawn);
		});
		if (isLearningDesire && !job.playerForced)
		{
			toil.defaultCompleteMode = ToilCompleteMode.Never;
		}
		return toil;
	}

	private Toil CarryToReadingSpot()
	{
		Toil toil = ToilMaker.MakeToil("CarryToReadingSpot");
		toil.initAction = delegate
		{
			if (!TryGetClosestChairFreeSittingSpot(skipInteractionCells: true, out var cell) && !TryGetClosestChairFreeSittingSpot(skipInteractionCells: false, out cell))
			{
				cell = RCellFinder.SpotToChewStandingNear(pawn, Book, (IntVec3 c) => !c.Fogged(pawn.Map) && pawn.CanReserveSittableOrSpot(c));
			}
			if (!cell.IsValid)
			{
				pawn.pather.StartPath(pawn.Position, PathEndMode.OnCell);
			}
			else
			{
				pawn.ReserveSittableOrSpot(cell, pawn.CurJob);
				pawn.Map.pawnDestinationReservationManager.Reserve(pawn, pawn.CurJob, cell);
				pawn.pather.StartPath(cell, PathEndMode.OnCell);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		return toil;
	}

	private bool TryGetClosestChairFreeSittingSpot(bool skipInteractionCells, out IntVec3 cell)
	{
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(pawn), 32f, (Thing t) => ValidateChair(t, pawn, skipInteractionCells) && t.Position.GetDangerFor(pawn, t.Map) == Danger.None);
		if (thing != null)
		{
			return TryFindFreeSittingSpotOnThing(thing, pawn, skipInteractionCells, out cell);
		}
		cell = IntVec3.Invalid;
		return false;
	}

	private Toil FindAdjacentReadingSurface()
	{
		Toil toil = ToilMaker.MakeToil("FindAdjacentReadingSurface");
		toil.initAction = delegate
		{
			Map map = pawn.Map;
			IntVec3 position = pawn.Position;
			Building firstThing = pawn.Position.GetFirstThing<Building>(pawn.Map);
			if (firstThing != null && firstThing.def.building != null && firstThing.def.building.isSittable)
			{
				if (!TryFaceClosestSurface(position, map))
				{
					job.SetTarget(TargetIndex.B, position + firstThing.Rotation.FacingCell);
					pawn.jobs.curDriver.rotateToFace = TargetIndex.B;
				}
			}
			else
			{
				TryFaceClosestSurface(position, map);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	private bool TryFaceClosestSurface(IntVec3 pos, Map map)
	{
		for (int i = 0; i < 4; i++)
		{
			IntVec3 intVec = pos + new Rot4(i).FacingCell;
			if (intVec.GetSurfaceType(map) == SurfaceType.Eat)
			{
				job.SetTarget(TargetIndex.B, intVec);
				pawn.jobs.curDriver.rotateToFace = TargetIndex.B;
				return true;
			}
		}
		for (int j = 0; j < 4; j++)
		{
			IntVec3 intVec2 = pos + new Rot4(j).FacingCell;
			if (intVec2.GetSurfaceType(map) == SurfaceType.Item)
			{
				job.SetTarget(TargetIndex.B, intVec2);
				pawn.jobs.curDriver.rotateToFace = TargetIndex.B;
				return true;
			}
		}
		return false;
	}

	private static bool ValidateChair(Thing t, Pawn pawn, bool skipInteractionCells)
	{
		if (t.def.building == null || !t.def.building.isSittable)
		{
			return false;
		}
		if (!TryFindFreeSittingSpotOnThing(t, pawn, skipInteractionCells, out var _))
		{
			return false;
		}
		if (t.Fogged())
		{
			return false;
		}
		if (t.IsForbidden(pawn))
		{
			return false;
		}
		if (!pawn.CanReserve(t))
		{
			return false;
		}
		if (!t.IsSociallyProper(pawn))
		{
			return false;
		}
		if (t.IsBurning())
		{
			return false;
		}
		if (t.HostileTo(pawn))
		{
			return false;
		}
		return true;
	}

	private static bool TryFindFreeSittingSpotOnThing(Thing t, Pawn pawn, bool skipInteractionCells, out IntVec3 cell)
	{
		foreach (IntVec3 item in t.OccupiedRect())
		{
			if ((!skipInteractionCells || !item.IsBuildingInteractionCell(pawn.Map)) && !item.Fogged(pawn.Map) && pawn.CanReserveSittableOrSpot(item))
			{
				cell = item;
				return true;
			}
		}
		cell = default(IntVec3);
		return false;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref carrying, "carrying", defaultValue: false);
		Scribe_Values.Look(ref hasInInventory, "hasInInventory", defaultValue: false);
		Scribe_Values.Look(ref isLearningDesire, "wasLearningDesire", defaultValue: false);
	}
}
