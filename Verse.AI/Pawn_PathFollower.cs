using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI
{
	public class Pawn_PathFollower : IExposable
	{
		protected Pawn pawn;

		private bool moving;

		public IntVec3 nextCell;

		private IntVec3 lastCell;

		public float nextCellCostLeft;

		public float nextCellCostTotal = 1f;

		private int cellsUntilClamor;

		private int lastMovedTick = -999999;

		private LocalTargetInfo destination;

		private PathEndMode peMode;

		public PawnPath curPath;

		public IntVec3 lastPathedTargetPosition;

		private int foundPathWhichCollidesWithPawns = -999999;

		private int foundPathWithDanger = -999999;

		private int failedToFindCloseUnoccupiedCellTicks = -999999;

		private const int MaxMoveTicks = 450;

		private const int MaxCheckAheadNodes = 20;

		private const float SnowReductionFromWalking = 0.001f;

		private const int ClamorCellsInterval = 12;

		private const int MinCostWalk = 50;

		private const int MinCostAmble = 60;

		private const float StaggerMoveSpeedFactor = 0.17f;

		private const int CheckForMovingCollidingPawnsIfCloserToTargetThanX = 30;

		private const int AttackBlockingHostilePawnAfterTicks = 180;

		public LocalTargetInfo Destination => destination;

		public bool Moving => moving;

		public bool MovingNow
		{
			get
			{
				if (Moving)
				{
					return !WillCollideWithPawnOnNextPathCell();
				}
				return false;
			}
		}

		public IntVec3 LastPassableCellInPath
		{
			get
			{
				if (!Moving || curPath == null)
				{
					return IntVec3.Invalid;
				}
				if (!Destination.Cell.Impassable(pawn.Map))
				{
					return Destination.Cell;
				}
				List<IntVec3> nodesReversed = curPath.NodesReversed;
				for (int i = 0; i < nodesReversed.Count; i++)
				{
					if (!nodesReversed[i].Impassable(pawn.Map))
					{
						return nodesReversed[i];
					}
				}
				if (!pawn.Position.Impassable(pawn.Map))
				{
					return pawn.Position;
				}
				return IntVec3.Invalid;
			}
		}

		public Pawn_PathFollower(Pawn newPawn)
		{
			pawn = newPawn;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref moving, "moving", defaultValue: true);
			Scribe_Values.Look(ref nextCell, "nextCell");
			Scribe_Values.Look(ref nextCellCostLeft, "nextCellCostLeft", 0f);
			Scribe_Values.Look(ref nextCellCostTotal, "nextCellCostInitial", 0f);
			Scribe_Values.Look(ref peMode, "peMode", PathEndMode.None);
			Scribe_Values.Look(ref cellsUntilClamor, "cellsUntilClamor", 0);
			Scribe_Values.Look(ref lastMovedTick, "lastMovedTick", -999999);
			if (moving)
			{
				Scribe_TargetInfo.Look(ref destination, "destination");
			}
		}

		public void StartPath(LocalTargetInfo dest, PathEndMode peMode)
		{
			dest = (LocalTargetInfo)GenPath.ResolvePathMode(pawn, dest.ToTargetInfo(pawn.Map), ref peMode);
			if (dest.HasThing && dest.ThingDestroyed)
			{
				Log.Error(string.Concat(pawn, " pathing to destroyed thing ", dest.Thing));
				PatherFailed();
			}
			else
			{
				if ((!PawnCanOccupy(pawn.Position) && !TryRecoverFromUnwalkablePosition()) || (moving && curPath != null && destination == dest && this.peMode == peMode))
				{
					return;
				}
				if (!pawn.Map.reachability.CanReach(pawn.Position, dest, peMode, TraverseParms.For(TraverseMode.PassDoors)))
				{
					PatherFailed();
					return;
				}
				this.peMode = peMode;
				destination = dest;
				if (!IsNextCellWalkable() || NextCellDoorToWaitForOrManuallyOpen() != null || nextCellCostLeft == nextCellCostTotal)
				{
					ResetToCurrentPosition();
				}
				PawnDestinationReservationManager.PawnDestinationReservation pawnDestinationReservation = pawn.Map.pawnDestinationReservationManager.MostRecentReservationFor(pawn);
				if (pawnDestinationReservation != null && ((destination.HasThing && pawnDestinationReservation.target != destination.Cell) || (pawnDestinationReservation.job != pawn.CurJob && pawnDestinationReservation.target != destination.Cell)))
				{
					pawn.Map.pawnDestinationReservationManager.ObsoleteAllClaimedBy(pawn);
				}
				if (AtDestinationPosition())
				{
					PatherArrived();
					return;
				}
				if (pawn.Downed)
				{
					Log.Error(pawn.LabelCap + " tried to path while downed. This should never happen. curJob=" + pawn.CurJob.ToStringSafe());
					PatherFailed();
					return;
				}
				if (curPath != null)
				{
					curPath.ReleaseToPool();
				}
				curPath = null;
				moving = true;
				pawn.jobs.posture = PawnPosture.Standing;
			}
		}

		public void StopDead()
		{
			if (curPath != null)
			{
				curPath.ReleaseToPool();
			}
			curPath = null;
			moving = false;
			nextCell = pawn.Position;
		}

		public void PatherTick()
		{
			if (WillCollideWithPawnAt(this.pawn.Position))
			{
				if (FailedToFindCloseUnoccupiedCellRecently())
				{
					return;
				}
				if (CellFinder.TryFindBestPawnStandCell(this.pawn, out var cell, cellByCell: true) && cell != this.pawn.Position)
				{
					this.pawn.Position = cell;
					ResetToCurrentPosition();
					if (moving && TrySetNewPath())
					{
						TryEnterNextPathCell();
					}
				}
				else
				{
					failedToFindCloseUnoccupiedCellTicks = Find.TickManager.TicksGame;
				}
			}
			else
			{
				if (this.pawn.stances.FullBodyBusy)
				{
					return;
				}
				if (moving && WillCollideWithPawnOnNextPathCell())
				{
					nextCellCostLeft = nextCellCostTotal;
					if (((curPath != null && curPath.NodesLeftCount < 30) || PawnUtility.AnyPawnBlockingPathAt(nextCell, this.pawn, actAsIfHadCollideWithPawnsJob: false, collideOnlyWithStandingPawns: true)) && !BestPathHadPawnsInTheWayRecently() && TrySetNewPath())
					{
						ResetToCurrentPosition();
						TryEnterNextPathCell();
					}
					else if (Find.TickManager.TicksGame - lastMovedTick >= 180)
					{
						Pawn pawn = PawnUtility.PawnBlockingPathAt(nextCell, this.pawn);
						if (pawn != null && this.pawn.HostileTo(pawn) && this.pawn.TryGetAttackVerb(pawn) != null)
						{
							Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, pawn);
							job.maxNumMeleeAttacks = 1;
							job.expiryInterval = 300;
							this.pawn.jobs.StartJob(job, JobCondition.Incompletable);
						}
					}
				}
				else
				{
					lastMovedTick = Find.TickManager.TicksGame;
					if (nextCellCostLeft > 0f)
					{
						nextCellCostLeft -= CostToPayThisTick();
					}
					else if (moving)
					{
						TryEnterNextPathCell();
					}
				}
			}
		}

		public void TryResumePathingAfterLoading()
		{
			if (moving)
			{
				StartPath(destination, peMode);
			}
		}

		public void Notify_Teleported_Int()
		{
			StopDead();
			ResetToCurrentPosition();
		}

		public void ResetToCurrentPosition()
		{
			nextCell = pawn.Position;
			nextCellCostLeft = 0f;
			nextCellCostTotal = 1f;
		}

		private bool PawnCanOccupy(IntVec3 c)
		{
			if (!c.Walkable(pawn.Map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(pawn.Map);
			if (edifice != null)
			{
				Building_Door building_Door = edifice as Building_Door;
				if (building_Door != null && !building_Door.PawnCanOpen(pawn) && !building_Door.Open)
				{
					return false;
				}
			}
			return true;
		}

		public Building BuildingBlockingNextPathCell()
		{
			Building edifice = nextCell.GetEdifice(pawn.Map);
			if (edifice != null && edifice.BlocksPawn(pawn))
			{
				return edifice;
			}
			return null;
		}

		public bool WillCollideWithPawnOnNextPathCell()
		{
			return WillCollideWithPawnAt(nextCell);
		}

		private bool IsNextCellWalkable()
		{
			if (!nextCell.Walkable(pawn.Map))
			{
				return false;
			}
			if (WillCollideWithPawnAt(nextCell))
			{
				return false;
			}
			return true;
		}

		private bool WillCollideWithPawnAt(IntVec3 c)
		{
			if (!PawnUtility.ShouldCollideWithPawns(pawn))
			{
				return false;
			}
			return PawnUtility.AnyPawnBlockingPathAt(c, pawn);
		}

		public Building_Door NextCellDoorToWaitForOrManuallyOpen()
		{
			Building_Door building_Door = pawn.Map.thingGrid.ThingAt<Building_Door>(nextCell);
			if (building_Door != null && building_Door.SlowsPawns && (!building_Door.Open || building_Door.TicksTillFullyOpened > 0) && building_Door.PawnCanOpen(pawn))
			{
				return building_Door;
			}
			return null;
		}

		public void PatherDraw()
		{
			if (DebugViewSettings.drawPaths && curPath != null && Find.Selector.IsSelected(pawn))
			{
				curPath.DrawPath(pawn);
			}
		}

		public bool MovedRecently(int ticks)
		{
			return Find.TickManager.TicksGame - lastMovedTick <= ticks;
		}

		public bool TryRecoverFromUnwalkablePosition(bool error = true)
		{
			bool flag = false;
			for (int i = 0; i < GenRadial.RadialPattern.Length; i++)
			{
				IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
				if (!PawnCanOccupy(intVec))
				{
					continue;
				}
				if (intVec == pawn.Position)
				{
					return true;
				}
				if (error)
				{
					Log.Warning(string.Concat(pawn, " on unwalkable cell ", pawn.Position, ". Teleporting to ", intVec));
				}
				pawn.Position = intVec;
				pawn.Notify_Teleported(endCurrentJob: true, resetTweenedPos: false);
				flag = true;
				break;
			}
			if (!flag)
			{
				pawn.Destroy();
				Log.Error(string.Concat(pawn.ToStringSafe(), " on unwalkable cell ", pawn.Position, ". Could not find walkable position nearby. Destroyed."));
			}
			return flag;
		}

		private void PatherArrived()
		{
			StopDead();
			if (pawn.jobs.curJob != null)
			{
				pawn.jobs.curDriver.Notify_PatherArrived();
			}
		}

		private void PatherFailed()
		{
			StopDead();
			pawn.jobs.curDriver.Notify_PatherFailed();
		}

		private void TryEnterNextPathCell()
		{
			Building building = BuildingBlockingNextPathCell();
			if (building != null)
			{
				Building_Door building_Door = building as Building_Door;
				if (building_Door == null || !building_Door.FreePassage)
				{
					if ((pawn.CurJob != null && pawn.CurJob.canBash) || pawn.HostileTo(building))
					{
						Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, building);
						job.expiryInterval = 300;
						pawn.jobs.StartJob(job, JobCondition.Incompletable);
					}
					else
					{
						PatherFailed();
					}
					return;
				}
			}
			Building_Door building_Door2 = NextCellDoorToWaitForOrManuallyOpen();
			if (building_Door2 != null)
			{
				if (!building_Door2.Open)
				{
					building_Door2.StartManualOpenBy(pawn);
				}
				Stance_Cooldown stance_Cooldown = new Stance_Cooldown(building_Door2.TicksTillFullyOpened, building_Door2, null);
				stance_Cooldown.neverAimWeapon = true;
				pawn.stances.SetStance(stance_Cooldown);
				building_Door2.CheckFriendlyTouched(pawn);
				return;
			}
			lastCell = pawn.Position;
			pawn.Position = nextCell;
			if (pawn.RaceProps.Humanlike)
			{
				cellsUntilClamor--;
				if (cellsUntilClamor <= 0)
				{
					GenClamor.DoClamor(pawn, 7f, ClamorDefOf.Movement);
					cellsUntilClamor = 12;
				}
			}
			pawn.filth.Notify_EnteredNewCell();
			if (pawn.BodySize > 0.9f)
			{
				pawn.Map.snowGrid.AddDepth(pawn.Position, -0.001f);
			}
			Building_Door building_Door3 = pawn.Map.thingGrid.ThingAt<Building_Door>(lastCell);
			if (building_Door3 != null && !pawn.HostileTo(building_Door3))
			{
				building_Door3.CheckFriendlyTouched(pawn);
				if (!building_Door3.BlockedOpenMomentary && !building_Door3.HoldOpen && building_Door3.SlowsPawns && building_Door3.PawnCanOpen(pawn))
				{
					building_Door3.StartManualCloseBy(pawn);
					return;
				}
			}
			if (!NeedNewPath() || TrySetNewPath())
			{
				if (AtDestinationPosition())
				{
					PatherArrived();
				}
				else
				{
					SetupMoveIntoNextCell();
				}
			}
		}

		private void SetupMoveIntoNextCell()
		{
			if (curPath.NodesLeftCount <= 1)
			{
				Log.Error(string.Concat(pawn, " at ", pawn.Position, " ran out of path nodes while pathing to ", destination, "."));
				PatherFailed();
				return;
			}
			nextCell = curPath.ConsumeNextNode();
			if (!nextCell.Walkable(pawn.Map))
			{
				Log.Error(string.Concat(pawn, " entering ", nextCell, " which is unwalkable."));
			}
			int num = CostToMoveIntoCell(nextCell);
			nextCellCostTotal = num;
			nextCellCostLeft = num;
			pawn.Map.thingGrid.ThingAt<Building_Door>(nextCell)?.Notify_PawnApproaching(pawn, num);
		}

		private int CostToMoveIntoCell(IntVec3 c)
		{
			return CostToMoveIntoCell(pawn, c);
		}

		private static int CostToMoveIntoCell(Pawn pawn, IntVec3 c)
		{
			int num = ((c.x != pawn.Position.x && c.z != pawn.Position.z) ? pawn.TicksPerMoveDiagonal : pawn.TicksPerMoveCardinal);
			num += pawn.Map.pathGrid.CalculatedCostAt(c, perceivedStatic: false, pawn.Position);
			Building edifice = c.GetEdifice(pawn.Map);
			if (edifice != null)
			{
				num += edifice.PathWalkCostFor(pawn);
			}
			if (num > 450)
			{
				num = 450;
			}
			if (pawn.CurJob != null)
			{
				Pawn locomotionUrgencySameAs = pawn.jobs.curDriver.locomotionUrgencySameAs;
				if (locomotionUrgencySameAs != null && locomotionUrgencySameAs != pawn && locomotionUrgencySameAs.Spawned)
				{
					int num2 = CostToMoveIntoCell(locomotionUrgencySameAs, c);
					if (num < num2)
					{
						num = num2;
					}
				}
				else
				{
					switch (pawn.jobs.curJob.locomotionUrgency)
					{
					case LocomotionUrgency.Amble:
						num *= 3;
						if (num < 60)
						{
							num = 60;
						}
						break;
					case LocomotionUrgency.Walk:
						num *= 2;
						if (num < 50)
						{
							num = 50;
						}
						break;
					case LocomotionUrgency.Jog:
						num = num;
						break;
					case LocomotionUrgency.Sprint:
						num = Mathf.RoundToInt((float)num * 0.75f);
						break;
					}
				}
			}
			return Mathf.Max(num, 1);
		}

		private float CostToPayThisTick()
		{
			float num = 1f;
			if (pawn.stances.Staggered)
			{
				num *= 0.17f;
			}
			if (num < nextCellCostTotal / 450f)
			{
				num = nextCellCostTotal / 450f;
			}
			return num;
		}

		private bool TrySetNewPath()
		{
			PawnPath pawnPath = GenerateNewPath();
			if (!pawnPath.Found)
			{
				PatherFailed();
				return false;
			}
			if (curPath != null)
			{
				curPath.ReleaseToPool();
			}
			curPath = pawnPath;
			for (int i = 0; i < 20 && i < curPath.NodesLeftCount; i++)
			{
				IntVec3 c = curPath.Peek(i);
				if (PawnUtility.ShouldCollideWithPawns(pawn) && PawnUtility.AnyPawnBlockingPathAt(c, pawn))
				{
					foundPathWhichCollidesWithPawns = Find.TickManager.TicksGame;
				}
				if (PawnUtility.KnownDangerAt(c, pawn.Map, pawn))
				{
					foundPathWithDanger = Find.TickManager.TicksGame;
				}
				if (foundPathWhichCollidesWithPawns == Find.TickManager.TicksGame && foundPathWithDanger == Find.TickManager.TicksGame)
				{
					break;
				}
			}
			return true;
		}

		private PawnPath GenerateNewPath()
		{
			lastPathedTargetPosition = destination.Cell;
			return pawn.Map.pathFinder.FindPath(pawn.Position, destination, pawn, peMode);
		}

		private bool AtDestinationPosition()
		{
			return pawn.CanReachImmediate(destination, peMode);
		}

		private bool NeedNewPath()
		{
			if (!destination.IsValid || curPath == null || !curPath.Found || curPath.NodesLeftCount == 0)
			{
				return true;
			}
			if (destination.HasThing && destination.Thing.Map != pawn.Map)
			{
				return true;
			}
			if ((pawn.Position.InHorDistOf(curPath.LastNode, 15f) || pawn.Position.InHorDistOf(destination.Cell, 15f)) && !ReachabilityImmediate.CanReachImmediate(curPath.LastNode, destination, pawn.Map, peMode, pawn))
			{
				return true;
			}
			if (curPath.UsedRegionHeuristics && curPath.NodesConsumedCount >= 75)
			{
				return true;
			}
			if (lastPathedTargetPosition != destination.Cell)
			{
				float num = (pawn.Position - destination.Cell).LengthHorizontalSquared;
				float num2 = ((num > 900f) ? 10f : ((num > 289f) ? 5f : ((num > 100f) ? 3f : ((!(num > 49f)) ? 0.5f : 2f))));
				if ((float)(lastPathedTargetPosition - destination.Cell).LengthHorizontalSquared > num2 * num2)
				{
					return true;
				}
			}
			bool flag = PawnUtility.ShouldCollideWithPawns(pawn);
			bool flag2 = curPath.NodesLeftCount < 30;
			IntVec3 other = IntVec3.Invalid;
			for (int i = 0; i < 20 && i < curPath.NodesLeftCount; i++)
			{
				IntVec3 intVec = curPath.Peek(i);
				if (!intVec.Walkable(pawn.Map))
				{
					return true;
				}
				if (flag && !BestPathHadPawnsInTheWayRecently() && (PawnUtility.AnyPawnBlockingPathAt(intVec, pawn, actAsIfHadCollideWithPawnsJob: false, collideOnlyWithStandingPawns: true) || (flag2 && PawnUtility.AnyPawnBlockingPathAt(intVec, pawn))))
				{
					return true;
				}
				if (!BestPathHadDangerRecently() && PawnUtility.KnownDangerAt(intVec, pawn.Map, pawn))
				{
					return true;
				}
				Building_Door building_Door = intVec.GetEdifice(pawn.Map) as Building_Door;
				if (building_Door != null)
				{
					if (!building_Door.CanPhysicallyPass(pawn) && !pawn.HostileTo(building_Door))
					{
						return true;
					}
					if (building_Door.IsForbiddenToPass(pawn))
					{
						return true;
					}
				}
				if (i != 0 && intVec.AdjacentToDiagonal(other) && (PathFinder.BlocksDiagonalMovement(intVec.x, other.z, pawn.Map) || PathFinder.BlocksDiagonalMovement(other.x, intVec.z, pawn.Map)))
				{
					return true;
				}
				other = intVec;
			}
			return false;
		}

		private bool BestPathHadPawnsInTheWayRecently()
		{
			return foundPathWhichCollidesWithPawns + 240 > Find.TickManager.TicksGame;
		}

		private bool BestPathHadDangerRecently()
		{
			return foundPathWithDanger + 240 > Find.TickManager.TicksGame;
		}

		private bool FailedToFindCloseUnoccupiedCellRecently()
		{
			return failedToFindCloseUnoccupiedCellTicks + 100 > Find.TickManager.TicksGame;
		}
	}
}
