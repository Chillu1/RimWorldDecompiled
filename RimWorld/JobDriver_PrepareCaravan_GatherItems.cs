using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobDriver_PrepareCaravan_GatherItems : JobDriver
{
	private int pickedUpFirstItemTicks = -1;

	private int toilLoops;

	private PrepareCaravanGatherState gatherState;

	private const TargetIndex ToHaulInd = TargetIndex.A;

	private const TargetIndex CarrierInd = TargetIndex.B;

	private const int MaxTicksGatherItems = 7500;

	private const int LoopBackstop = 500;

	public Thing ToHaul => job.GetTarget(TargetIndex.A).Thing;

	public Pawn Carrier => (Pawn)job.GetTarget(TargetIndex.B).Thing;

	private List<TransferableOneWay> Transferables => ((LordJob_FormAndSendCaravan)job.lord.LordJob).transferables;

	private TransferableOneWay Transferable
	{
		get
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(ToHaul, Transferables, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay != null)
			{
				return transferableOneWay;
			}
			throw new InvalidOperationException("Could not find any matching transferable.");
		}
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(ToHaul, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (gatherState == PrepareCaravanGatherState.Unset)
		{
			gatherState = ((pawn.IsFormingCaravan() && (!MassUtility.IsOverEncumbered(pawn) || pawn.inventory.HasAnyUnpackedCaravanItems)) ? PrepareCaravanGatherState.Haul : PrepareCaravanGatherState.Carry);
		}
		if (gatherState == PrepareCaravanGatherState.Carry)
		{
			return MakeNewToilsCarry();
		}
		return MakeNewToilsHaulInInventory();
	}

	private IEnumerable<Toil> MakeNewToilsCarry()
	{
		this.FailOn(() => !base.Map.lordManager.lords.Contains(job.lord));
		Toil reserve = Toils_Reserve.Reserve(TargetIndex.A).FailOnDestroyedOrNull(TargetIndex.A);
		yield return reserve;
		bool inInventory = HaulAIUtility.IsInHaulableInventory(ToHaul);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, inInventory);
		yield return DetermineNumToHaul();
		yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true, failIfStackCountLessThanJobCount: false, reserve: true, inInventory);
		yield return AddCarriedThingToTransferables();
		yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserve, TargetIndex.A, TargetIndex.None, takeFromValidStorage: true, (Thing x) => Transferable.things.Contains(x));
		Toil findCarrier = FindCarrier();
		yield return findCarrier;
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).JumpIf(() => !IsUsableCarrier(Carrier, pawn, allowColonists: true), findCarrier);
		yield return Toils_General.Wait(25).JumpIf(() => !IsUsableCarrier(Carrier, pawn, allowColonists: true), findCarrier).WithProgressBarToilDelay(TargetIndex.B);
		yield return PlaceTargetInCarrierInventory();
	}

	private IEnumerable<Toil> MakeNewToilsHaulInInventory()
	{
		this.FailOn(() => !base.Map.lordManager.lords.Contains(job.lord));
		bool inInventory = HaulAIUtility.IsInHaulableInventory(ToHaul);
		Toil reserve = Toils_Reserve.Reserve(TargetIndex.A).FailOnDestroyedOrNull(TargetIndex.A);
		Toil findCarrier = FindCarrier();
		yield return reserve;
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, inInventory).JumpIf(IsFinishedCollectingItems, findCarrier);
		yield return DetermineNumToHaul(findCarrier);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true, failIfStackCountLessThanJobCount: false, reserve: true, inInventory);
		yield return AddCarriedThingToTransferables();
		yield return Toils_General.Wait(25).WithProgressBarToilDelay(TargetIndex.B);
		yield return HaulCaravanItemInInventory(reserve);
		yield return findCarrier;
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).JumpIf(() => !IsUsableCarrier(Carrier, pawn, allowColonists: true), findCarrier);
		yield return Toils_General.Wait(25).JumpIf(() => !IsUsableCarrier(Carrier, pawn, allowColonists: true), findCarrier).WithProgressBarToilDelay(TargetIndex.B);
		yield return AddHauledItemsToCarrier(findCarrier);
	}

	private Toil DetermineNumToHaul(Toil findCarrier = null)
	{
		Toil toil = ToilMaker.MakeToil("DetermineNumToHaul");
		toil.initAction = delegate
		{
			int num = GatherItemsForCaravanUtility.CountLeftToTransfer(pawn, Transferable, job.lord);
			if (pawn.carryTracker.CarriedThing != null)
			{
				num -= pawn.carryTracker.CarriedThing.stackCount;
			}
			if (num <= 0)
			{
				if (findCarrier == null || !pawn.inventory.HasAnyUnpackedCaravanItems)
				{
					pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
				else
				{
					pawn.jobs.curDriver.JumpToToil(findCarrier);
				}
			}
			else
			{
				job.count = num;
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	private Toil AddCarriedThingToTransferables()
	{
		Toil toil = ToilMaker.MakeToil("AddCarriedThingToTransferables");
		toil.initAction = delegate
		{
			TransferableOneWay transferable = Transferable;
			if (!transferable.things.Contains(pawn.carryTracker.CarriedThing))
			{
				transferable.things.Add(pawn.carryTracker.CarriedThing);
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		toil.atomicWithPrevious = true;
		return toil;
	}

	private Toil FindCarrier()
	{
		Toil toil = ToilMaker.MakeToil("FindCarrier");
		toil.initAction = delegate
		{
			Pawn pawn = FindBestCarrier(onlyAnimals: true);
			if (pawn == null)
			{
				bool flag = base.pawn.GetLord() == job.lord;
				if (flag && !MassUtility.IsOverEncumbered(base.pawn))
				{
					pawn = base.pawn;
				}
				else
				{
					pawn = FindBestCarrier(onlyAnimals: false);
					if (pawn == null)
					{
						if (flag)
						{
							pawn = base.pawn;
						}
						else
						{
							IEnumerable<Pawn> source = job.lord.ownedPawns.Where((Pawn x) => IsUsableCarrier(x, base.pawn, allowColonists: true));
							if (!source.Any())
							{
								EndJobWith(JobCondition.Incompletable);
								return;
							}
							pawn = source.RandomElement();
						}
					}
				}
			}
			job.SetTarget(TargetIndex.B, pawn);
		};
		return toil;
	}

	private bool IsFinishedCollectingItems()
	{
		if (!MassUtility.IsOverEncumbered(pawn))
		{
			if (pickedUpFirstItemTicks > -1)
			{
				return Find.TickManager.TicksGame > pickedUpFirstItemTicks + 7500;
			}
			return false;
		}
		return true;
	}

	private Toil HaulCaravanItemInInventory(Toil reserve)
	{
		Toil toil = ToilMaker.MakeToil("HaulCaravanItemInInventory");
		toil.initAction = delegate
		{
			if (pickedUpFirstItemTicks == -1)
			{
				pickedUpFirstItemTicks = Find.TickManager.TicksGame;
			}
			Transferable.AdjustTo(Mathf.Max(Transferable.CountToTransfer - pawn.carryTracker.CarriedThing.stackCount, 0));
			pawn.inventory.AddHauledCaravanItem(pawn.carryTracker.CarriedThing);
			if (!IsFinishedCollectingItems())
			{
				SetNewHaulTargetAndJumpToReserve(reserve);
			}
		};
		return toil;
	}

	private void SetNewHaulTargetAndJumpToReserve(Toil reserve)
	{
		if (CheckToilLoopBackstop())
		{
			Thing thing = GatherItemsForCaravanUtility.FindThingToHaul(pawn, pawn.GetLord());
			if (thing != null)
			{
				job.SetTarget(TargetIndex.A, thing);
				pawn.jobs.curDriver.JumpToToil(reserve);
			}
		}
	}

	private Toil AddHauledItemsToCarrier(Toil findCarrier)
	{
		Toil toil = ToilMaker.MakeToil("AddHauledItemsToCarrier");
		toil.initAction = delegate
		{
			if (Carrier == pawn)
			{
				pawn.inventory.ClearHaulingCaravanCache();
			}
			else
			{
				pawn.inventory.TransferCaravanItemsToCarrier(Carrier.inventory);
				if (pawn.inventory.HasAnyUnpackedCaravanItems && CheckToilLoopBackstop())
				{
					pawn.jobs.curDriver.JumpToToil(findCarrier);
				}
			}
		};
		return toil;
	}

	private bool CheckToilLoopBackstop()
	{
		if (++toilLoops > 500)
		{
			Log.Error("Prepare caravan gather items job for pawn " + pawn.Label + " looped through toils too many times");
			EndJobWith(JobCondition.Errored);
			return false;
		}
		return true;
	}

	private Toil PlaceTargetInCarrierInventory()
	{
		Toil toil = ToilMaker.MakeToil("PlaceTargetInCarrierInventory");
		toil.initAction = delegate
		{
			Pawn_CarryTracker carryTracker = pawn.carryTracker;
			Thing carriedThing = carryTracker.CarriedThing;
			if (carryTracker.innerContainer.Count == 0)
			{
				carryTracker.pawn.Drawer.renderer.SetAllGraphicsDirty();
			}
			Transferable.AdjustTo(Mathf.Max(Transferable.CountToTransfer - carriedThing.stackCount, 0));
			carryTracker.innerContainer.TryTransferToContainer(carriedThing, Carrier.inventory.innerContainer, carriedThing.stackCount, out var resultingTransferredItem);
			CompForbiddable compForbiddable = resultingTransferredItem?.TryGetComp<CompForbiddable>();
			if (compForbiddable != null)
			{
				compForbiddable.Forbidden = false;
			}
		};
		return toil;
	}

	public static bool IsUsableCarrier(Pawn p, Pawn forPawn, bool allowColonists)
	{
		if (!p.IsFormingCaravan())
		{
			return false;
		}
		if (p == forPawn)
		{
			return true;
		}
		if (p.DestroyedOrNull() || !p.Spawned || p.inventory.UnloadEverything || !forPawn.CanReach(p, PathEndMode.Touch, Danger.Deadly))
		{
			return false;
		}
		if (allowColonists && p.IsColonist)
		{
			return true;
		}
		if ((p.RaceProps.packAnimal || p.HostFaction == Faction.OfPlayer) && !p.IsBurning() && !p.Downed)
		{
			return !MassUtility.IsOverEncumbered(p);
		}
		return false;
	}

	private float GetCarrierScore(Pawn p)
	{
		float lengthHorizontal = (p.Position - pawn.Position).LengthHorizontal;
		float num = MassUtility.EncumbrancePercent(p);
		return 1f - num - lengthHorizontal / 10f * 0.2f;
	}

	private Pawn FindBestCarrier(bool onlyAnimals)
	{
		Lord lord = job.lord;
		Pawn pawn = null;
		float num = 0f;
		if (lord != null)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn2 = lord.ownedPawns[i];
				if (pawn2 != base.pawn && (!onlyAnimals || pawn2.RaceProps.Animal) && IsUsableCarrier(pawn2, base.pawn, allowColonists: false))
				{
					float carrierScore = GetCarrierScore(pawn2);
					if (pawn == null || carrierScore > num)
					{
						pawn = pawn2;
						num = carrierScore;
					}
				}
			}
		}
		return pawn;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref pickedUpFirstItemTicks, "pickedUpFirstItemTicks", 0);
		Scribe_Values.Look(ref toilLoops, "toilLoops", 0);
		Scribe_Values.Look(ref gatherState, "gatherState", PrepareCaravanGatherState.Unset);
	}
}
