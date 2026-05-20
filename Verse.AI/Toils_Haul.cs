using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse.AI
{
	public class Toils_Haul
	{
		public static bool ErrorCheckForCarry(Pawn pawn, Thing haulThing, bool canTakeFromInventory = false)
		{
			if (!haulThing.SpawnedOrAnyParentSpawned || (!canTakeFromInventory && !haulThing.Spawned))
			{
				Log.Message(pawn?.ToString() + " tried to start carry " + haulThing?.ToString() + " which isn't spawned.");
				pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				return true;
			}
			if (haulThing.stackCount == 0)
			{
				Log.Message(pawn?.ToString() + " tried to start carry " + haulThing?.ToString() + " which had stackcount 0.");
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

		public static Toil StartCarryThing(TargetIndex haulableInd, bool putRemainderInQueue = false, bool subtractNumTakenFromJobCount = false, bool failIfStackCountLessThanJobCount = false, bool reserve = true, bool canTakeFromInventory = false)
		{
			Toil toil = ToilMaker.MakeToil("StartCarryThing");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(haulableInd).Thing;
				if (!ErrorCheckForCarry(actor, thing, canTakeFromInventory))
				{
					if (curJob.count == 0)
					{
						throw new Exception($"StartCarryThing job had count = {curJob.count}. Job: {curJob}");
					}
					int num = actor.carryTracker.AvailableStackSpace(thing.def);
					if (num <= 0)
					{
						int num2 = actor.carryTracker.MaxStackSpaceEver(thing.def);
						int num3 = 0;
						if (actor.carryTracker.CarriedThing != null)
						{
							num3 = actor.carryTracker.CarriedThing.stackCount;
						}
						throw new Exception($"StartCarryThing got availableStackSpace {num} (haulTarg {thing}, Job: {curJob}, maximum: {num2}, carrying: {num3})");
					}
					if (failIfStackCountLessThanJobCount && thing.stackCount < curJob.count)
					{
						actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
					}
					else
					{
						int num4 = Mathf.Min(curJob.count, num, thing.stackCount);
						if (num4 <= 0)
						{
							int num5 = actor.carryTracker.MaxStackSpaceEver(thing.def);
							int num6 = 0;
							if (actor.carryTracker.CarriedThing != null)
							{
								num6 = actor.carryTracker.CarriedThing.stackCount;
							}
							throw new Exception($"StartCarryThing zero or negative desiredNumToTake ({num4}), actor: {actor}, curJob.count: {curJob.count}, availableStackSpace: {num} (maximum: {num5}, carrying: {num6}), haulTarg.stackCount: {thing.stackCount}");
						}
						int stackCount = thing.stackCount;
						int num7 = actor.carryTracker.TryStartCarry(thing, num4, reserve);
						if (num7 == 0)
						{
							actor.jobs.EndCurrentJob(JobCondition.Incompletable);
						}
						if (num7 < stackCount)
						{
							int num8 = curJob.count - num7;
							if (putRemainderInQueue && num8 > 0)
							{
								curJob.GetTargetQueue(haulableInd).Insert(0, thing);
								Job job = curJob;
								if (job.countQueue == null)
								{
									job.countQueue = new List<int>();
								}
								curJob.countQueue.Insert(0, num8);
							}
							else if (actor.Map.reservationManager.ReservedBy(thing, actor, curJob))
							{
								actor.Map.reservationManager.Release(thing, actor, curJob);
							}
						}
						if (subtractNumTakenFromJobCount)
						{
							curJob.count -= num7;
						}
						curJob.SetTarget(haulableInd, actor.carryTracker.CarriedThing);
						actor.records.Increment(RecordDefOf.ThingsHauled);
					}
				}
			};
			return toil;
		}

		public static Toil StoreThingJob(TargetIndex thingIndex)
		{
			Toil toil = ToilMaker.MakeToil("StoreThingJob");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.CurJob;
				Thing thing = curJob.GetTarget(thingIndex).Thing;
				Job job = HaulAIUtility.HaulToStorageJob(actor, thing, curJob.playerForced);
				if (job != null)
				{
					actor.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				}
			};
			return toil;
		}

		public static Toil DropCarriedThing()
		{
			Toil toil = ToilMaker.MakeToil("DropCarriedThing");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Thing resultingThing;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(actor?.ToString() + " tried to drop carried thing but is not carrying anything.");
				}
				else if (!actor.carryTracker.TryDropCarriedThing(actor.Position, ThingPlaceMode.Direct, out resultingThing))
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
			};
			return toil;
		}

		public static Toil JumpIfAlsoCollectingNextTargetInQueue(Toil gotoGetTargetToil, TargetIndex ind)
		{
			Toil toil = ToilMaker.MakeToil("JumpIfAlsoCollectingNextTargetInQueue");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
				if (!targetQueue.NullOrEmpty() && curJob.count > 0)
				{
					if (actor.carryTracker.CarriedThing == null)
					{
						Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor?.ToString() + " who is not carrying something.");
					}
					else if (actor.carryTracker.AvailableStackSpace(actor.carryTracker.CarriedThing.def) > 0)
					{
						for (int i = 0; i < targetQueue.Count; i++)
						{
							if (!GenAI.CanUseItemForWork(actor, targetQueue[i].Thing))
							{
								actor.jobs.EndCurrentJob(JobCondition.Incompletable);
								break;
							}
							if (targetQueue[i].Thing.def == actor.carryTracker.CarriedThing.def)
							{
								curJob.SetTarget(ind, targetQueue[i].Thing);
								targetQueue.RemoveAt(i);
								actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
								break;
							}
						}
					}
				}
			};
			return toil;
		}

		public static Toil CheckForGetOpportunityDuplicate(Toil getHaulTargetToil, TargetIndex haulableInd, TargetIndex storeCellInd, bool takeFromValidStorage = false, Predicate<Thing> extraValidator = null)
		{
			Toil toil = ToilMaker.MakeToil("CheckForGetOpportunityDuplicate");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carryTracker.CarriedThing.def.stackLimit != 1 && !actor.carryTracker.Full && curJob.count > 0)
				{
					Thing thing = GenClosest.ClosestThingReachable(actor.Position, actor.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), PathEndMode.ClosestTouch, TraverseParms.For(actor), 8f, DupeValidator);
					if (thing != null)
					{
						curJob.SetTarget(haulableInd, thing);
						actor.jobs.curDriver.JumpToToil(getHaulTargetToil);
					}
				}
				bool DupeValidator(Thing t)
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
					if (t.stackCount >= t.def.stackLimit)
					{
						return false;
					}
					if (t.IsForbidden(actor))
					{
						return false;
					}
					if (!t.IsSociallyProper(actor, forPrisoner: false, animalsCare: true))
					{
						return false;
					}
					LocalTargetInfo localTargetInfo = ((storeCellInd != TargetIndex.None) ? curJob.GetTarget(storeCellInd) : LocalTargetInfo.Invalid);
					if (takeFromValidStorage && localTargetInfo.IsValid && localTargetInfo.Cell.TryGetSlotGroup(actor.Map, out var group) && t.TryGetValidStoragePriority(out var priority) && (int)priority >= (int)group.Settings.Priority)
					{
						return false;
					}
					if (localTargetInfo.IsValid && t.Position == localTargetInfo.Cell)
					{
						return false;
					}
					if (localTargetInfo.IsValid && !localTargetInfo.Cell.IsValidStorageFor(actor.Map, t))
					{
						return false;
					}
					if (!actor.CanReserve(t))
					{
						return false;
					}
					if (extraValidator != null && !extraValidator(t))
					{
						return false;
					}
					return true;
				}
			};
			return toil;
		}

		public static Toil CarryHauledThingToCell(TargetIndex squareIndex, PathEndMode pathEndMode = PathEndMode.ClosestTouch)
		{
			Toil toil = ToilMaker.MakeToil("CarryHauledThingToCell");
			toil.initAction = delegate
			{
				IntVec3 cell = toil.actor.jobs.curJob.GetTarget(squareIndex).Cell;
				toil.actor.pather.StartPath(cell, pathEndMode);
			};
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			toil.AddEndCondition(delegate
			{
				Pawn actor = toil.actor;
				IntVec3 cell = actor.jobs.curJob.GetTarget(squareIndex).Cell;
				CompPushable compPushable = actor.carryTracker.CarriedThing.TryGetComp<CompPushable>();
				if (compPushable != null)
				{
					Vector3 v = actor.Position.ToVector3() + compPushable.drawPos;
					if (new IntVec3(v) == cell)
					{
						return JobCondition.Succeeded;
					}
				}
				return JobCondition.Ongoing;
			});
			toil.AddFailCondition(delegate
			{
				Pawn actor = toil.actor;
				IntVec3 cell = actor.jobs.curJob.GetTarget(squareIndex).Cell;
				if (actor.carryTracker.CarriedThing == null)
				{
					return true;
				}
				if (actor.jobs.curJob.haulMode == HaulMode.ToCellStorage && !cell.IsValidStorageFor(actor.Map, actor.carryTracker.CarriedThing))
				{
					return true;
				}
				CompPushable compPushable = actor.carryTracker.CarriedThing.TryGetComp<CompPushable>();
				return (compPushable != null && !compPushable.canBePushed) ? true : false;
			});
			return toil;
		}

		public static Toil PlaceCarriedThingInCellFacing(TargetIndex facingTargetInd)
		{
			Toil toil = ToilMaker.MakeToil("PlaceCarriedThingInCellFacing");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(actor?.ToString() + " tried to place hauled thing in facing cell but is not hauling anything.");
				}
				else
				{
					LocalTargetInfo target = actor.CurJob.GetTarget(facingTargetInd);
					IntVec3 intVec = ((!target.HasThing) ? target.Cell : target.Thing.OccupiedRect().ClosestCellTo(actor.Position));
					IntVec3 dropLoc = actor.Position + Pawn_RotationTracker.RotFromAngleBiased((actor.Position - intVec).AngleFlat).FacingCell;
					if (!actor.carryTracker.TryDropCarriedThing(dropLoc, ThingPlaceMode.Direct, out var _))
					{
						actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
				}
			};
			return toil;
		}

		public static Toil PlaceHauledThingInCell(TargetIndex cellInd, Toil nextToilOnPlaceFailOrIncomplete, bool storageMode, bool tryStoreInSameStorageIfSpotCantHoldWholeStack = false)
		{
			Toil toil = ToilMaker.MakeToil("PlaceHauledThingInCell");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				IntVec3 cell = curJob.GetTarget(cellInd).Cell;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(actor?.ToString() + " tried to place hauled thing in cell but is not hauling anything.");
				}
				else
				{
					SlotGroup slotGroup = actor.Map.haulDestinationManager.SlotGroupAt(cell);
					if (slotGroup != null && slotGroup.Settings.AllowedToAccept(actor.carryTracker.CarriedThing))
					{
						actor.Map.designationManager.TryRemoveDesignationOn(actor.carryTracker.CarriedThing, DesignationDefOf.Haul);
					}
					Action<Thing, int> placedAction = null;
					if (curJob.def == JobDefOf.DoBill || curJob.def == JobDefOf.RecolorApparel || curJob.def == JobDefOf.RefuelAtomic || curJob.def == JobDefOf.RearmTurretAtomic)
					{
						placedAction = delegate(Thing th, int added)
						{
							HaulAIUtility.UpdateJobWithPlacedThings(curJob, th, added);
						};
					}
					if (!actor.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Direct, out var _, placedAction))
					{
						if (storageMode)
						{
							IntVec3 storeCell;
							if (nextToilOnPlaceFailOrIncomplete != null && ((tryStoreInSameStorageIfSpotCantHoldWholeStack && StoreUtility.TryFindBestBetterStoreCellForIn(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, curJob.bill.GetSlotGroup(), out var foundCell)) || StoreUtility.TryFindBestBetterStoreCellFor(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, out foundCell)))
							{
								if (actor.CanReserve(foundCell))
								{
									actor.Reserve(foundCell, actor.CurJob);
								}
								actor.CurJob.SetTarget(cellInd, foundCell);
								actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
							}
							else if (HaulAIUtility.CanHaulAside(actor, actor.carryTracker.CarriedThing, out storeCell))
							{
								curJob.SetTarget(cellInd, storeCell);
								curJob.count = int.MaxValue;
								curJob.haulOpportunisticDuplicates = false;
								curJob.haulMode = HaulMode.ToCellNonStorage;
								if (nextToilOnPlaceFailOrIncomplete != null)
								{
									actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
								}
							}
							else
							{
								Log.Warning($"Incomplete haul for {actor}: Could not find anywhere to put {actor.carryTracker.CarriedThing} near {actor.Position}. Destroying. This should be very uncommon!");
								actor.carryTracker.CarriedThing.Destroy();
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
			Toil gotoDest = ToilMaker.MakeToil("CarryHauledThingToContainer");
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

		public static Toil DepositHauledThingInContainer(TargetIndex containerInd, TargetIndex reserveForContainerInd, Action onDeposited = null)
		{
			Toil toil = ToilMaker.MakeToil("DepositHauledThingInContainer");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carryTracker.CarriedThing == null)
				{
					Log.Error(actor?.ToString() + " tried to place hauled thing in container but is not hauling anything.");
				}
				else
				{
					Thing thing = curJob.GetTarget(containerInd).Thing;
					ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
					if (thingOwner != null)
					{
						int num = actor.carryTracker.CarriedThing.stackCount;
						if (thing is IHaulEnroute haulEnroute)
						{
							ThingDef def = actor.carryTracker.CarriedThing.def;
							num = Mathf.Min(haulEnroute.GetSpaceRemainingWithEnroute(def, actor), num);
							if (reserveForContainerInd != TargetIndex.None)
							{
								Thing thing2 = curJob.GetTarget(reserveForContainerInd).Thing;
								if (!thing2.DestroyedOrNull() && thing2 != haulEnroute && thing2 is IHaulEnroute enroute)
								{
									int spaceRemainingWithEnroute = enroute.GetSpaceRemainingWithEnroute(def, actor);
									num = Mathf.Min(num, actor.carryTracker.CarriedThing.stackCount - spaceRemainingWithEnroute);
								}
							}
						}
						Thing carriedThing = actor.carryTracker.CarriedThing;
						int num2 = actor.carryTracker.innerContainer.TryTransferToContainer(carriedThing, thingOwner, num);
						if (num2 != 0)
						{
							if (thing is IHaulEnroute container)
							{
								thing.Map.enrouteManager.ReleaseFor(container, actor);
							}
							if (thing is INotifyHauledTo notifyHauledTo)
							{
								notifyHauledTo.Notify_HauledTo(actor, carriedThing, num2);
							}
							if (thing is ThingWithComps thingWithComps)
							{
								foreach (ThingComp allComp in thingWithComps.AllComps)
								{
									if (allComp is INotifyHauledTo notifyHauledTo2)
									{
										notifyHauledTo2.Notify_HauledTo(actor, carriedThing, num2);
									}
								}
							}
							if (curJob.def == JobDefOf.DoBill)
							{
								HaulAIUtility.UpdateJobWithPlacedThings(curJob, carriedThing, num2);
							}
							onDeposited?.Invoke();
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
			Toil toil = ToilMaker.MakeToil("JumpToCarryToNextContainerIfPossible");
			toil.debugName = "Jump carry if possible";
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				if (actor.carryTracker.CarriedThing != null && curJob.targetQueueB != null && curJob.targetQueueB.Count > 0)
				{
					if (TryGetNextDestinationFromQueue(primaryTargetInd, TargetIndex.B, actor.carryTracker.CarriedThing.def, curJob, actor, out var nextTarget))
					{
						curJob.targetQueueB.RemoveAll((LocalTargetInfo target) => target.Thing == nextTarget);
						curJob.targetB = nextTarget;
						curJob.targetC = nextTarget;
						actor.jobs.curDriver.JumpToToil(carryToContainerToil);
					}
				}
			};
			return toil;
		}

		public static bool TryGetNextDestinationFromQueue(TargetIndex primaryIndex, TargetIndex destIndex, ThingDef stuff, Job job, Pawn actor, out Thing target)
		{
			Thing primaryTarget = job.GetTarget(primaryIndex).Thing;
			target = null;
			if (actor.carryTracker?.CarriedThing == null)
			{
				return false;
			}
			bool hasSpareItems = actor.carryTracker.CarriedThing.stackCount > 0;
			if (primaryTarget != null && primaryTarget.Spawned && primaryTarget is IHaulEnroute enroute)
			{
				int spaceRemainingWithEnroute = enroute.GetSpaceRemainingWithEnroute(stuff, actor);
				hasSpareItems = actor.carryTracker.CarriedThing.stackCount > spaceRemainingWithEnroute;
			}
			target = GenClosest.ClosestThing_Global_Reachable(actor.Position, actor.Map, from x in job.GetTargetQueue(destIndex)
				select x.Thing, PathEndMode.Touch, TraverseParms.For(actor), 99999f, Validator);
			return target != null;
			bool Validator(Thing th)
			{
				if (!(th is IHaulEnroute enroute2))
				{
					return false;
				}
				if (enroute2.GetSpaceRemainingWithEnroute(stuff, actor) <= 0)
				{
					return false;
				}
				if (th != primaryTarget && !hasSpareItems)
				{
					return false;
				}
				return true;
			}
		}

		public static Toil TakeToInventory(TargetIndex ind, int count)
		{
			return TakeToInventory(ind, count, null, null);
		}

		private static Toil TakeToInventory(TargetIndex ind, int? count, Func<int> countGetter, Func<Thing, int> countGetterPassingThing)
		{
			Toil takeThing = ToilMaker.MakeToil("TakeToInventory");
			takeThing.initAction = delegate
			{
				Pawn actor = takeThing.actor;
				Thing thing = actor.CurJob.GetTarget(ind).Thing;
				if (!ErrorCheckForCarry(actor, thing))
				{
					int num = Mathf.Min(count ?? countGetterPassingThing?.Invoke(thing) ?? countGetter(), thing.stackCount);
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

		public static Toil TakeToInventory(TargetIndex ind, Func<int> countGetter)
		{
			return TakeToInventory(ind, null, countGetter, null);
		}

		public static Toil TakeToInventory(TargetIndex ind, Func<Thing, int> countGetter)
		{
			return TakeToInventory(ind, null, null, countGetter);
		}

		public static Toil TakeFromOtherInventory(Thing item, ThingOwner taker, ThingOwner holder, int count = -1, TargetIndex indexToSet = TargetIndex.None)
		{
			Toil toil = ToilMaker.MakeToil("TakeFromOtherInventory");
			toil.initAction = delegate
			{
				if (!holder.Contains(item))
				{
					toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else
				{
					count = ((count < 0) ? toil.actor.jobs.curJob.count : count);
					holder.TryTransferToContainer(item, taker, Mathf.Min(item.stackCount, count), out var resultingTransferredItem);
					if (resultingTransferredItem == null)
					{
						Log.Warning($"Taker {toil.actor.Label} unable to take count {count} of thing {item.Label} from holder's inventory");
						toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					else if (indexToSet != TargetIndex.None)
					{
						toil.actor.jobs.curJob.SetTarget(indexToSet, resultingTransferredItem);
					}
				}
			};
			return toil;
		}

		public static Toil CheckItemCarriedByOtherPawn(Thing item, TargetIndex targetPawnIfCarried = TargetIndex.None, Toil jumpIfCarriedByOther = null)
		{
			Toil toil = ToilMaker.MakeToil("CheckItemCarriedByOtherPawn");
			toil.initAction = delegate
			{
				Pawn pawn = (item?.ParentHolder as Pawn_InventoryTracker)?.pawn;
				if (pawn != null && pawn != toil.actor)
				{
					if (targetPawnIfCarried != TargetIndex.None)
					{
						toil.actor.jobs.curJob.SetTarget(targetPawnIfCarried, pawn);
					}
					if (jumpIfCarriedByOther != null)
					{
						toil.actor.jobs.curDriver.JumpToToil(jumpIfCarriedByOther);
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.atomicWithPrevious = true;
			return toil;
		}
	}
}
