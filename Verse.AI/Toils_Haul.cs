using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse.Sound;

namespace Verse.AI
{
	public class Toils_Haul
	{
		public static bool ErrorCheckForCarry(Pawn pawn, Thing haulThing)
		{
			if (!haulThing.Spawned)
			{
				Log.Message(string.Concat(pawn, " tried to start carry ", haulThing, " which isn't spawned."));
				pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				return true;
			}
			if (haulThing.stackCount == 0)
			{
				Log.Message(string.Concat(pawn, " tried to start carry ", haulThing, " which had stackcount 0."));
				pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				return true;
			}
			if (pawn.jobs.curJob.count <= 0)
			{
				Log.Error("Invalid count: " + pawn.jobs.curJob.count + ", setting to 1. Job was " + pawn.jobs.curJob);
				pawn.jobs.curJob.count = 1;
			}
			return false;
		}

		public static Toil StartCarryThing(TargetIndex haulableInd, bool putRemainderInQueue = false, bool subtractNumTakenFromJobCount = false, bool failIfStackCountLessThanJobCount = false)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(haulableInd).Thing;
				if (!ErrorCheckForCarry(actor, thing))
				{
					if (curJob.count == 0)
					{
						throw new Exception("StartCarryThing job had count = " + curJob.count + ". Job: " + curJob);
					}
					int num = actor.carryTracker.AvailableStackSpace(thing.def);
					if (num == 0)
					{
						throw new Exception(string.Concat("StartCarryThing got availableStackSpace ", num, " for haulTarg ", thing, ". Job: ", curJob));
					}
					if (failIfStackCountLessThanJobCount && thing.stackCount < curJob.count)
					{
						actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
					}
					else
					{
						int num2 = Mathf.Min(curJob.count, num, thing.stackCount);
						if (num2 <= 0)
						{
							throw new Exception("StartCarryThing desiredNumToTake = " + num2);
						}
						int stackCount = thing.stackCount;
						int num3 = actor.carryTracker.TryStartCarry(thing, num2);
						if (num3 == 0)
						{
							actor.jobs.EndCurrentJob(JobCondition.Incompletable);
						}
						if (num3 < stackCount)
						{
							int num4 = curJob.count - num3;
							if (putRemainderInQueue && num4 > 0)
							{
								curJob.GetTargetQueue(haulableInd).Insert(0, thing);
								if (curJob.countQueue == null)
								{
									curJob.countQueue = new List<int>();
								}
								curJob.countQueue.Insert(0, num4);
							}
							else if (actor.Map.reservationManager.ReservedBy(thing, actor, curJob))
							{
								actor.Map.reservationManager.Release(thing, actor, curJob);
							}
						}
						if (subtractNumTakenFromJobCount)
						{
							curJob.count -= num3;
						}
						curJob.SetTarget(haulableInd, actor.carryTracker.CarriedThing);
						actor.records.Increment(RecordDefOf.ThingsHauled);
					}
				}
			};
			return toil;
		}

		public static Toil JumpIfAlsoCollectingNextTargetInQueue(Toil gotoGetTargetToil, TargetIndex ind)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
				if (!targetQueue.NullOrEmpty() && curJob.count > 0)
				{
					if (actor.carryTracker.CarriedThing == null)
					{
						Log.Error(string.Concat("JumpToAlsoCollectTargetInQueue run on ", actor, " who is not carrying something."));
					}
					else if (actor.carryTracker.AvailableStackSpace(actor.carryTracker.CarriedThing.def) > 0)
					{
						int num = 0;
						while (true)
						{
							if (num >= targetQueue.Count)
							{
								return;
							}
							if (!GenAI.CanUseItemForWork(actor, targetQueue[num].Thing))
							{
								actor.jobs.EndCurrentJob(JobCondition.Incompletable);
								return;
							}
							if (targetQueue[num].Thing.def == actor.carryTracker.CarriedThing.def)
							{
								break;
							}
							num++;
						}
						curJob.SetTarget(ind, targetQueue[num].Thing);
						targetQueue.RemoveAt(num);
						actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
					}
				}
			};
			return toil;
		}

		public static Toil CheckForGetOpportunityDuplicate(Toil getHaulTargetToil, TargetIndex haulableInd, TargetIndex storeCellInd, bool takeFromValidStorage = false, Predicate<Thing> extraValidator = null)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carryTracker.CarriedThing.def.stackLimit != 1 && !actor.carryTracker.Full && curJob.count > 0)
				{
					Thing thing = null;
					Predicate<Thing> validator = delegate(Thing t)
					{
						if (!t.Spawned)
						{
							return false;
						}
						if (t.def != actor.carryTracker.CarriedThing.def)
						{
							return false;
						}
						if (!t.CanStackWith(actor.carryTracker.CarriedThing))
						{
							return false;
						}
						if (t.IsForbidden(actor))
						{
							return false;
						}
						if (!takeFromValidStorage && t.IsInValidStorage())
						{
							return false;
						}
						if (storeCellInd != 0 && !curJob.GetTarget(storeCellInd).Cell.IsValidStorageFor(actor.Map, t))
						{
							return false;
						}
						if (!actor.CanReserve(t))
						{
							return false;
						}
						return (extraValidator == null || extraValidator(t)) ? true : false;
					};
					thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.ClosestTouch, TraverseParms.For(actor), 8f, validator);
					if (thing != null)
					{
						curJob.SetTarget(haulableInd, thing);
						actor.jobs.curDriver.JumpToToil(getHaulTargetToil);
					}
				}
			};
			return toil;
		}

		public static Toil CarryHauledThingToCell(TargetIndex squareIndex)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				IntVec3 cell2 = toil.actor.jobs.curJob.GetTarget(squareIndex).Cell;
				toil.actor.pather.StartPath(cell2, PathEndMode.ClosestTouch);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.AddFailCondition(delegate
			{
				Pawn actor = toil.actor;
				IntVec3 cell = actor.jobs.curJob.GetTarget(squareIndex).Cell;
				return (actor.jobs.curJob.haulMode == HaulMode.ToCellStorage && !cell.IsValidStorageFor(actor.Map, actor.carryTracker.CarriedThing)) ? true : false;
			});
			return toil;
		}

		public static Toil PlaceCarriedThingInCellFacing(TargetIndex facingTargetInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(string.Concat(actor, " tried to place hauled thing in facing cell but is not hauling anything."));
				}
				else
				{
					LocalTargetInfo target = actor.CurJob.GetTarget(facingTargetInd);
					IntVec3 b = (!target.HasThing) ? target.Cell : target.Thing.OccupiedRect().ClosestCellTo(actor.Position);
					IntVec3 dropLoc = actor.Position + Pawn_RotationTracker.RotFromAngleBiased((actor.Position - b).AngleFlat).FacingCell;
					if (!actor.carryTracker.TryDropCarriedThing(dropLoc, ThingPlaceMode.Direct, out Thing _))
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
				}
			};
			return toil;
		}

		public static Toil PlaceHauledThingInCell(TargetIndex cellInd, Toil nextToilOnPlaceFailOrIncomplete, bool storageMode, bool tryStoreInSameStorageIfSpotCantHoldWholeStack = false)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				IntVec3 cell = curJob.GetTarget(cellInd).Cell;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(string.Concat(actor, " tried to place hauled thing in cell but is not hauling anything."));
				}
				else
				{
					SlotGroup slotGroup = actor.Map.haulDestinationManager.SlotGroupAt(cell);
					if (slotGroup != null && slotGroup.Settings.AllowedToAccept(actor.carryTracker.CarriedThing))
					{
						actor.Map.designationManager.TryRemoveDesignationOn(actor.carryTracker.CarriedThing, DesignationDefOf.Haul);
					}
					Action<Thing, int> placedAction = null;
					if (curJob.def == JobDefOf.DoBill || curJob.def == JobDefOf.RefuelAtomic || curJob.def == JobDefOf.RearmTurretAtomic)
					{
						placedAction = delegate(Thing th, int added)
						{
							if (curJob.placedThings == null)
							{
								curJob.placedThings = new List<ThingCountClass>();
							}
							ThingCountClass thingCountClass = curJob.placedThings.Find((ThingCountClass x) => x.thing == th);
							if (thingCountClass != null)
							{
								thingCountClass.Count += added;
							}
							else
							{
								curJob.placedThings.Add(new ThingCountClass(th, added));
							}
						};
					}
					if (!actor.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Direct, out Thing _, placedAction))
					{
						if (storageMode)
						{
							if (nextToilOnPlaceFailOrIncomplete != null && ((tryStoreInSameStorageIfSpotCantHoldWholeStack && StoreUtility.TryFindBestBetterStoreCellForIn(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, cell.GetSlotGroup(actor.Map), out IntVec3 foundCell)) || StoreUtility.TryFindBestBetterStoreCellFor(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, out foundCell)))
							{
								if (actor.CanReserve(foundCell))
								{
									actor.Reserve(foundCell, actor.CurJob);
								}
								actor.CurJob.SetTarget(cellInd, foundCell);
								actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
							}
							else
							{
								Job job = HaulAIUtility.HaulAsideJobFor(actor, actor.carryTracker.CarriedThing);
								if (job != null)
								{
									curJob.targetA = job.targetA;
									curJob.targetB = job.targetB;
									curJob.targetC = job.targetC;
									curJob.count = job.count;
									curJob.haulOpportunisticDuplicates = job.haulOpportunisticDuplicates;
									curJob.haulMode = job.haulMode;
									actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
								}
								else
								{
									Log.Error(string.Concat("Incomplete haul for ", actor, ": Could not find anywhere to put ", actor.carryTracker.CarriedThing, " near ", actor.Position, ". Destroying. This should never happen!"));
									actor.carryTracker.CarriedThing.Destroy();
								}
							}
						}
						else if (nextToilOnPlaceFailOrIncomplete != null)
						{
							actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
						}
					}
				}
			};
			return toil;
		}

		public static Toil CarryHauledThingToContainer()
		{
			Toil gotoDest = new Toil();
			gotoDest.initAction = delegate
			{
				gotoDest.actor.pather.StartPath(gotoDest.actor.jobs.curJob.targetB.Thing, PathEndMode.Touch);
			};
			gotoDest.AddFailCondition(delegate
			{
				Thing thing = gotoDest.actor.jobs.curJob.targetB.Thing;
				if (thing.Destroyed || (!gotoDest.actor.jobs.curJob.ignoreForbidden && thing.IsForbidden(gotoDest.actor)))
				{
					return true;
				}
				ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
				return (thingOwner != null && !thingOwner.CanAcceptAnyOf(gotoDest.actor.carryTracker.CarriedThing)) ? true : false;
			});
			gotoDest.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return gotoDest;
		}

		public static Toil DepositHauledThingInContainer(TargetIndex containerInd, TargetIndex reserveForContainerInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(string.Concat(actor, " tried to place hauled thing in container but is not hauling anything."));
				}
				else
				{
					Thing thing = curJob.GetTarget(containerInd).Thing;
					ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
					if (thingOwner != null)
					{
						int num = actor.carryTracker.CarriedThing.stackCount;
						if (thing is IConstructible)
						{
							num = Mathf.Min(GenConstruct.AmountNeededByOf((IConstructible)thing, actor.carryTracker.CarriedThing.def), num);
							if (reserveForContainerInd != 0)
							{
								Thing thing2 = curJob.GetTarget(reserveForContainerInd).Thing;
								if (thing2 != null && thing2 != thing)
								{
									int num2 = GenConstruct.AmountNeededByOf((IConstructible)thing2, actor.carryTracker.CarriedThing.def);
									num = Mathf.Min(num, actor.carryTracker.CarriedThing.stackCount - num2);
								}
							}
						}
						if (actor.carryTracker.innerContainer.TryTransferToContainer(actor.carryTracker.CarriedThing, thingOwner, num) != 0)
						{
							(thing as Building_Grave)?.Notify_CorpseBuried(actor);
						}
					}
					else if (curJob.GetTarget(containerInd).Thing.def.Minifiable)
					{
						actor.carryTracker.innerContainer.ClearAndDestroyContents();
					}
					else
					{
						Log.Error("Could not deposit hauled thing in container: " + curJob.GetTarget(containerInd).Thing);
					}
				}
			};
			return toil;
		}

		public static Toil JumpToCarryToNextContainerIfPossible(Toil carryToContainerToil, TargetIndex primaryTargetInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carryTracker.CarriedThing != null && curJob.targetQueueB != null && curJob.targetQueueB.Count > 0)
				{
					Thing primaryTarget = curJob.GetTarget(primaryTargetInd).Thing;
					bool hasSpareItems = actor.carryTracker.CarriedThing.stackCount > GenConstruct.AmountNeededByOf((IConstructible)primaryTarget, actor.carryTracker.CarriedThing.def);
					Predicate<Thing> validator = delegate(Thing th)
					{
						if (!GenConstruct.CanConstruct(th, actor, checkSkills: false))
						{
							return false;
						}
						if (!((IConstructible)th).MaterialsNeeded().Any((ThingDefCountClass need) => need.thingDef == actor.carryTracker.CarriedThing.def))
						{
							return false;
						}
						return (th == primaryTarget || hasSpareItems) ? true : false;
					};
					Thing nextTarget = GenClosest.ClosestThing_Global_Reachable(actor.Position, actor.Map, curJob.targetQueueB.Select((LocalTargetInfo targ) => targ.Thing), PathEndMode.Touch, TraverseParms.For(actor), 99999f, validator);
					if (nextTarget != null)
					{
						curJob.targetQueueB.RemoveAll((LocalTargetInfo targ) => targ.Thing == nextTarget);
						curJob.targetB = nextTarget;
						actor.jobs.curDriver.JumpToToil(carryToContainerToil);
					}
				}
			};
			return toil;
		}

		public static Toil TakeToInventory(TargetIndex ind, int count)
		{
			return TakeToInventory(ind, () => count);
		}

		public static Toil TakeToInventory(TargetIndex ind, Func<int> countGetter)
		{
			Toil takeThing = new Toil();
			takeThing.initAction = delegate
			{
				Pawn actor = takeThing.actor;
				Thing thing = actor.CurJob.GetTarget(ind).Thing;
				if (!ErrorCheckForCarry(actor, thing))
				{
					int num = Mathf.Min(countGetter(), thing.stackCount);
					if (actor.CurJob.checkEncumbrance)
					{
						num = Math.Min(num, MassUtility.CountToPickUpUntilOverEncumbered(actor, thing));
					}
					if (num <= 0)
					{
						actor.jobs.curDriver.ReadyForNextToil();
					}
					else
					{
						actor.inventory.GetDirectlyHeldThings().TryAdd(thing.SplitOff(num));
						if (thing.def.ingestible != null && (int)thing.def.ingestible.preferability <= 5)
						{
							actor.mindState.lastInventoryRawFoodUseTick = Find.TickManager.TicksGame;
						}
						thing.def.soundPickup.PlayOneShot(new TargetInfo(actor.Position, actor.Map));
					}
				}
			};
			return takeThing;
		}
	}
}
