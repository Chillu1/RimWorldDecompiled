using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobDriver_PrepareCaravan_GatherItems : JobDriver
	{
		private const TargetIndex ToHaulInd = TargetIndex.A;

		private const TargetIndex CarrierInd = TargetIndex.B;

		private const int PlaceInInventoryDuration = 25;

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
			this.FailOn(() => !base.Map.lordManager.lords.Contains(job.lord));
			Toil reserve = Toils_Reserve.Reserve(TargetIndex.A).FailOnDespawnedOrNull(TargetIndex.A);
			yield return reserve;
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return DetermineNumToHaul();
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
			yield return AddCarriedThingToTransferables();
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserve, TargetIndex.A, TargetIndex.None, takeFromValidStorage: true, (Thing x) => Transferable.things.Contains(x));
			Toil findCarrier = FindCarrier();
			yield return findCarrier;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).JumpIf(() => !IsUsableCarrier(Carrier, pawn, allowColonists: true), findCarrier);
			yield return Toils_General.Wait(25).JumpIf(() => !IsUsableCarrier(Carrier, pawn, allowColonists: true), findCarrier).WithProgressBarToilDelay(TargetIndex.B);
			yield return PlaceTargetInCarrierInventory();
		}

		private Toil DetermineNumToHaul()
		{
			return new Toil
			{
				initAction = delegate
				{
					int num = GatherItemsForCaravanUtility.CountLeftToTransfer(pawn, Transferable, job.lord);
					if (pawn.carryTracker.CarriedThing != null)
					{
						num -= pawn.carryTracker.CarriedThing.stackCount;
					}
					if (num <= 0)
					{
						pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
					}
					else
					{
						job.count = num;
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		private Toil AddCarriedThingToTransferables()
		{
			return new Toil
			{
				initAction = delegate
				{
					TransferableOneWay transferable = Transferable;
					if (!transferable.things.Contains(pawn.carryTracker.CarriedThing))
					{
						transferable.things.Add(pawn.carryTracker.CarriedThing);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}

		private Toil FindCarrier()
		{
			return new Toil
			{
				initAction = delegate
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
				}
			};
		}

		private Toil PlaceTargetInCarrierInventory()
		{
			return new Toil
			{
				initAction = delegate
				{
					Pawn_CarryTracker carryTracker = pawn.carryTracker;
					Thing carriedThing = carryTracker.CarriedThing;
					Transferable.AdjustTo(Mathf.Max(Transferable.CountToTransfer - carriedThing.stackCount, 0));
					carryTracker.innerContainer.TryTransferToContainer(carriedThing, Carrier.inventory.innerContainer, carriedThing.stackCount);
				}
			};
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
	}
}
