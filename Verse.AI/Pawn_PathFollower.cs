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

		public float lastMoveDirection;

		public float nextCellCostLeft;

		public float nextCellCostTotal = 1f;

		private int cellsUntilClamor;

		private int lastEnteredCellTick = -999999;

		private int lastMovedTick = -999999;

		public bool debugDisabled;

		public bool curPathJobIsStale;

		private LocalTargetInfo destination;

		private PathEndMode peMode;

		public PathRequest curPathRequest;

		public PawnPath curPath;

		public IntVec3 lastPathedTargetPosition;

		private int foundPathWhichCollidesWithPawns = -999999;

		private int foundPathWithDanger = -999999;

		private int failedToFindCloseUnoccupiedCellTicks = -999999;

		private float cachedMovePercentage;

		private bool cachedWillCollideNextCell;

		public bool debugLog;

		public bool cachedReturningToCell;

		private Pawn lastBlocker;

		private const int MaxMoveTicks = 450;

		private const int MaxCheckAheadNodes = 20;

		private const int CheckNeedNewPathIntervalTicks = 30;

		private const float SnowReductionFromWalking = 0.001f;

		private const int ClamorCellsInterval = 12;

		private const int MinCostWalk = 50;

		private const int MinCostAmble = 60;

		private const int CheckForMovingCollidingPawnsIfCloserToTargetThanX = 15;

		private const int AttackBlockingHostilePawnAfterTicks = 180;

		private const int WaitForRopeeTicks = 60;

		private const float RopeLength = 8f;

		public LocalTargetInfo Destination => destination;

		public bool Moving => moving;

		public bool MovingNow
		{
			get
			{
				if (Moving)
				{
					return !WillCollideNextCell;
				}
				return false;
			}
		}

		public float MovePercentage => cachedMovePercentage;

		public int LastMovedTick => lastMovedTick;

		public bool WillCollideNextCell => cachedWillCollideNextCell;

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
			Scribe_Values.Look(ref lastEnteredCellTick, "lastEnteredCellTick", -999999);
			Scribe_Values.Look(ref lastMovedTick, "lastMovedTick", -999999);
			Scribe_Values.Look(ref debugDisabled, "debugDisabled", defaultValue: false);
			Scribe_Values.Look(ref curPathJobIsStale, "curPathJobIsStale", defaultValue: false);
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
				Log.Error($"{pawn} pathing to destroyed thing {dest.Thing} curJob={pawn.CurJob.ToStringSafe()}");
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
				if (!IsNextCellWalkable() || NextCellDoorToWaitForOrManuallyOpen() != null || Mathf.Approximately(nextCellCostLeft, nextCellCostTotal) || (nextCell != pawn.Position && WillCollideWithPawnAt(nextCell)))
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
				if (pawn.Downed && !pawn.health.CanCrawl)
				{
					Log.Error(pawn.LabelCap + " tried to path while downed. This should never happen. curJob=" + pawn.CurJob.ToStringSafe());
					PatherFailed();
					return;
				}
				moving = true;
				pawn.jobs.posture = PawnPosture.Standing;
				cachedMovePercentage = 0f;
				cachedWillCollideNextCell = false;
				curPathJobIsStale = true;
				SetNewPathRequest();
			}
		}

		public void StopDead()
		{
			DisposeAndClearCurPathRequest();
			DisposeAndClearCurPath();
			moving = false;
			nextCell = pawn.Position;
			cachedMovePercentage = 0f;
			nextCellCostLeft = 0f;
			nextCellCostTotal = 1f;
			cachedWillCollideNextCell = false;
			curPathJobIsStale = false;
		}

		public void PatherTick()
		{
			if (this.pawn.RaceProps.doesntMove || debugDisabled)
			{
				return;
			}
			TrySetMovePercent();
			if (WillCollideWithPawnAt(this.pawn.Position, forceOnlyStanding: true, useId: true))
			{
				if (FailedToFindCloseUnoccupiedCellRecently())
				{
					return;
				}
				if (CellFinder.TryFindBestPawnStandCell(this.pawn, out var cell, cellByCell: true) && cell != this.pawn.Position)
				{
					if (DebugViewSettings.drawPatherState)
					{
						MoteMaker.ThrowText(this.pawn.DrawPos, this.pawn.Map, "Unstuck");
					}
					this.pawn.Position = cell;
					ResetToCurrentPosition();
					if (moving)
					{
						SetNewPathRequest();
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
				if (moving)
				{
					if (AtDestinationPosition())
					{
						PatherArrived();
						return;
					}
					if (curPathRequest != null && curPathRequest.TryGetPath(out var outPath))
					{
						curPathJobIsStale = false;
						if (curPathRequest.Found == false)
						{
							PatherFailed();
						}
						else if (!outPath.Found)
						{
							Log.Error($"{this.pawn} got an invalid path from path finder (maybe reachability and path finder state are out of sync).");
							outPath.Dispose();
							PatherFailed();
						}
						else
						{
							if (debugLog)
							{
								Log.Message($"{this.pawn}: Claimed new path: {outPath}");
							}
							DisposeAndClearCurPath();
							curPath = outPath;
							curPathRequest.ClaimCalculatedPath();
							DisposeAndClearCurPathRequest();
							List<IntVec3> list = curPath.PeekNextCells(20);
							for (int i = 0; i < list.Count; i++)
							{
								if (PawnUtility.KnownDangerAt(list[i], this.pawn.Map, this.pawn))
								{
									foundPathWithDanger = GenTicks.TicksGame;
									break;
								}
								if (WillCollideWithPawnAt(list[i]))
								{
									foundPathWhichCollidesWithPawns = GenTicks.TicksGame;
								}
							}
							lastMovedTick = GenTicks.TicksGame;
							if (!nextCell.IsValid || this.pawn.Position == nextCell)
							{
								SetupMoveIntoNextCell();
							}
						}
					}
					else if ((destination.IsValid && curPathRequest == null && curPath == null && !this.pawn.CanReachImmediate(destination, peMode)) || (this.pawn.IsHashIntervalTick(30) && NeedNewPath()))
					{
						if (debugLog)
						{
							Log.Message($"{this.pawn}: Needs a new path (maybe path blocked), will create new path request");
						}
						SetNewPathRequest();
					}
				}
				if (moving && curPath == null)
				{
					return;
				}
				cachedWillCollideNextCell = WillCollideWithPawnAt(nextCell);
				if (moving && WillCollideNextCell)
				{
					PawnPath pawnPath = curPath;
					bool flag = pawnPath != null && pawnPath.NodesLeftCount < 15;
					if (Find.TickManager.TicksGame - lastMovedTick >= 180 || lastBlocker != null)
					{
						Pawn pawn = PawnUtility.PawnBlockingPathAt(nextCell, this.pawn);
						if ((lastBlocker == null || lastBlocker == pawn) && pawn != null && this.pawn.HostileTo(pawn))
						{
							if (this.pawn.CanAttackWhenPathingBlocked && this.pawn.TryGetAttackVerb(pawn) != null)
							{
								Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, pawn);
								job.maxNumMeleeAttacks = 1;
								job.expiryInterval = 300;
								this.pawn.jobs.StartJob(job, JobCondition.Incompletable);
							}
							else
							{
								this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
							}
							return;
						}
					}
					cachedMovePercentage = 1f - nextCellCostLeft / nextCellCostTotal;
					if (flag && !BestPathHadPawnsInTheWayRecently())
					{
						lastBlocker = PawnUtility.PawnBlockingPathAt(nextCell, this.pawn);
						TrySetNewPathRequest();
					}
					return;
				}
				lastBlocker = null;
				if (curPath != null && (nextCellCostLeft > 0f || moving))
				{
					curPathJobIsStale = false;
					if (nextCellCostLeft > 0f)
					{
						nextCellCostLeft -= CostToPayThisTick();
					}
					if (nextCellCostLeft <= 0f)
					{
						TryEnterNextPathCell();
					}
					lastMovedTick = Find.TickManager.TicksGame;
					TrySetMovePercent();
				}
			}
		}

		private void TrySetMovePercent()
		{
			if (moving && pawn.Spawned && BuildingBlockingNextPathCell() == null && NextCellDoorToWaitForOrManuallyOpen() == null)
			{
				cachedMovePercentage = Mathf.Clamp01(1f - nextCellCostLeft / nextCellCostTotal);
			}
		}

		public void DrawDebugGUI()
		{
			Rect adjustedScreenspaceRect = SilhouetteUtility.GetAdjustedScreenspaceRect(pawn, 0.025f);
			Color? color = null;
			if (WillCollideWithPawnAt(pawn.Position, forceOnlyStanding: true))
			{
				color = new Color(0.14f, 0.93f, 1f, 0.5f);
			}
			else if (pawn.stances.FullBodyBusy)
			{
				color = new Color(0.37f, 1f, 0.19f, 0.5f);
			}
			else if (Moving && WillCollideNextCell)
			{
				color = new Color(1f, 0.71f, 0.22f, 0.5f);
			}
			if (color.HasValue)
			{
				GUI.DrawTexture(adjustedScreenspaceRect, TexUI.DotHighlight, ScaleMode.ScaleToFit, alphaBlend: true, 0f, color.Value, 0f, 0f);
			}
		}

		public void TryResumePathingAfterLoading()
		{
			if (moving)
			{
				if (destination.HasThing && destination.ThingDestroyed)
				{
					PatherFailed();
				}
				else
				{
					StartPath(destination, peMode);
				}
			}
		}

		public void Notify_Teleported_Int()
		{
			StopDead();
			ResetToCurrentPosition();
		}

		public void ResetToCurrentPosition()
		{
			DisposeAndClearCurPathRequest();
			DisposeAndClearCurPath();
			nextCell = pawn.Position;
			nextCellCostLeft = 0f;
			nextCellCostTotal = 1f;
			cachedMovePercentage = 0f;
			lastEnteredCellTick = GenTicks.TicksGame;
			if (moving && destination.IsValid)
			{
				SetNewPathRequest();
			}
		}

		private bool PawnCanOccupy(IntVec3 c)
		{
			if (!c.WalkableBy(pawn.Map, pawn))
			{
				return false;
			}
			Building_Door door = c.GetDoor(pawn.Map);
			if (door != null && !door.PawnCanOpen(pawn) && !door.Open)
			{
				return false;
			}
			return true;
		}

		private Building BuildingBlockingNextPathCell()
		{
			Building edifice = nextCell.GetEdifice(pawn.Map);
			if (edifice != null && edifice.BlocksPawn(pawn))
			{
				return edifice;
			}
			return null;
		}

		public void NotifyThingTransformed(Thing from, Thing to)
		{
			if (destination.HasThing && destination.Thing == from)
			{
				destination = new LocalTargetInfo(to);
			}
		}

		private bool IsNextCellWalkable()
		{
			if (!nextCell.WalkableBy(pawn.Map, pawn))
			{
				return false;
			}
			if (WillCollideWithPawnAt(nextCell))
			{
				return false;
			}
			return true;
		}

		private bool WillCollideWithPawnAt(IntVec3 c, bool forceOnlyStanding = false, bool useId = false)
		{
			if (!PawnUtility.ShouldCollideWithPawns(pawn))
			{
				return false;
			}
			return PawnUtility.AnyPawnBlockingPathAt(c, pawn, actAsIfHadCollideWithPawnsJob: false, forceOnlyStanding || (pawn.IsShambler && !pawn.mindState.anyCloseHostilesRecently), forPathFinder: false, useId);
		}

		public Building_Door NextCellDoorToWaitForOrManuallyOpen()
		{
			Building_Door door = nextCell.GetDoor(pawn.Map);
			if (door != null && door.SlowsPawns && (!door.Open || door.TicksTillFullyOpened > 0) && door.PawnCanOpen(pawn))
			{
				return door;
			}
			return null;
		}

		private Pawn RopeeWithStretchedRopeAtNextPathCell()
		{
			List<Pawn> ropees = this.pawn.roping.Ropees;
			for (int i = 0; i < ropees.Count; i++)
			{
				Pawn pawn = ropees[i];
				if (!pawn.Position.InHorDistOf(nextCell, 8f))
				{
					return pawn;
				}
			}
			return null;
		}

		public void PatherDraw()
		{
			if (!Find.ScreenshotModeHandler.Active && DebugViewSettings.drawPaths && curPath != null && Find.Selector.IsSelected(pawn) && !curPathJobIsStale)
			{
				curPath.DrawPath(pawn);
			}
		}

		public bool ChangedCellRecently(int ticks)
		{
			return Find.TickManager.TicksGame - lastEnteredCellTick <= ticks;
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
					Log.Warning($"{pawn} on unwalkable cell {pawn.Position}. Teleporting to {intVec}");
				}
				pawn.Position = intVec;
				pawn.Notify_Teleported(endCurrentJob: true, resetTweenedPos: false);
				flag = true;
				break;
			}
			if (!flag)
			{
				pawn.Destroy();
				Log.Error($"{pawn.ToStringSafe()} on unwalkable cell {pawn.Position}. Could not find walkable position nearby. Destroyed.");
			}
			return flag;
		}

		private void PatherArrived()
		{
			if (debugLog)
			{
				Log.Message($"{pawn}: Pather arrived");
			}
			StopDead();
			if (pawn.jobs.curJob != null && !curPathJobIsStale)
			{
				pawn.jobs.curDriver.Notify_PatherArrived();
			}
		}

		private void PatherFailed()
		{
			if (debugLog)
			{
				Log.Message($"{pawn}: Pather failed");
			}
			StopDead();
			if (pawn.jobs.curJob != null && !curPathJobIsStale)
			{
				pawn.jobs.curDriver.Notify_PatherFailed();
			}
		}

		private void TryEnterNextPathCell()
		{
			Building building = BuildingBlockingNextPathCell();
			if (building != null)
			{
				if (building is Building_Door { FreePassage: not false })
				{
					if (building.def.IsFence && this.pawn.FenceBlocked && this.pawn.CurJob != null && this.pawn.CurJob.canBashFences)
					{
						MakeBashBlockerJob(building);
					}
					else
					{
						ResetToCurrentPosition();
					}
				}
				else if ((this.pawn.CurJob != null && this.pawn.CurJob.canBashDoors) || this.pawn.HostileTo(building))
				{
					MakeBashBlockerJob(building);
				}
				else
				{
					ResetToCurrentPosition();
				}
			}
			else
			{
				if (WillCollideNextCell)
				{
					return;
				}
				Building_Door building_Door2 = NextCellDoorToWaitForOrManuallyOpen();
				if (building_Door2 != null)
				{
					if (building_Door2.IsForbiddenToPass(this.pawn))
					{
						TrySetNewPathRequest();
						return;
					}
					if (!building_Door2.Open)
					{
						building_Door2.StartManualOpenBy(this.pawn);
					}
					Stance_Cooldown stance = new Stance_Cooldown(building_Door2.TicksTillFullyOpened, building_Door2, null)
					{
						neverAimWeapon = true
					};
					this.pawn.stances.SetStance(stance);
					building_Door2.CheckFriendlyTouched(this.pawn);
					return;
				}
				lastCell = this.pawn.Position;
				this.pawn.Position = nextCell;
				lastMoveDirection = (nextCell - lastCell).AngleFlat;
				lastEnteredCellTick = GenTicks.TicksGame;
				if (this.pawn.RaceProps.Humanlike)
				{
					cellsUntilClamor--;
					if (cellsUntilClamor <= 0)
					{
						GenClamor.DoClamor(this.pawn, 7f, ClamorDefOf.Movement);
						cellsUntilClamor = 12;
					}
				}
				this.pawn.filth.Notify_EnteredNewCell();
				if (this.pawn.BodySize > 0.9f)
				{
					this.pawn.Map.snowGrid.AddDepth(this.pawn.Position, -0.001f);
				}
				Building_Door door = lastCell.GetDoor(this.pawn.Map);
				if (door != null && !this.pawn.HostileTo(door))
				{
					door.CheckFriendlyTouched(this.pawn);
					if (!door.BlockedOpenMomentary && !door.HoldOpen && door.SlowsPawns && door.PawnCanOpen(this.pawn))
					{
						door.StartManualCloseBy(this.pawn);
						return;
					}
				}
				Pawn pawn = RopeeWithStretchedRopeAtNextPathCell();
				if (pawn != null)
				{
					Stance_Cooldown stance_Cooldown = new Stance_Cooldown(60, pawn, null);
					stance_Cooldown.neverAimWeapon = true;
					this.pawn.stances.SetStance(stance_Cooldown);
				}
				else if (AtDestinationPosition())
				{
					PatherArrived();
				}
				else if (curPath.NodesLeftCount <= 1)
				{
					TrySetNewPathRequest();
				}
				else
				{
					SetupMoveIntoNextCell();
				}
			}
		}

		private void MakeBashBlockerJob(Building blocker)
		{
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, blocker);
			job.expiryInterval = 300;
			pawn.jobs.StartJob(job, JobCondition.Incompletable);
		}

		private void SetupMoveIntoNextCell()
		{
			if (curPath.NodesLeftCount <= 1)
			{
				Log.Error($"{pawn} at {pawn.Position} ran out of path nodes while pathing to {destination}.");
				PatherFailed();
				return;
			}
			IntVec3 intVec = nextCell;
			nextCell = curPath.ConsumeNextNode();
			if (intVec == nextCell)
			{
				if (curPath.NodesLeftCount <= 1)
				{
					if (AtDestinationPosition())
					{
						PatherArrived();
					}
					else
					{
						SetNewPathRequest();
					}
					return;
				}
				nextCell = curPath.ConsumeNextNode();
			}
			if (!nextCell.WalkableBy(pawn.Map, pawn))
			{
				TrySetNewPathRequest();
				return;
			}
			float num = (nextCellCostTotal = CostToMoveIntoCell(nextCell));
			nextCellCostLeft = Mathf.Max(num + Mathf.Min(nextCellCostLeft, 0f), 1f);
			cachedWillCollideNextCell = WillCollideWithPawnAt(nextCell);
			cachedMovePercentage = Mathf.Clamp01(1f - nextCellCostLeft / nextCellCostTotal);
			nextCell.GetDoor(pawn.Map)?.Notify_PawnApproaching(pawn, num);
		}

		private float CostToMoveIntoCell(IntVec3 c)
		{
			return CostToMoveIntoCell(pawn, c);
		}

		public static int? GetPawnCellBaseCostOverride(Pawn pawn, IntVec3 c)
		{
			if (c.GetTerrain(pawn.Map).IsWater && pawn.WaterCellCost.HasValue)
			{
				return pawn.WaterCellCost;
			}
			return null;
		}

		private static float CostToMoveIntoCell(Pawn pawn, IntVec3 c)
		{
			float num = ((c.x != pawn.Position.x && c.z != pawn.Position.z) ? pawn.TicksPerMoveDiagonal : pawn.TicksPerMoveCardinal);
			int? pawnCellBaseCostOverride = GetPawnCellBaseCostOverride(pawn, c);
			num += (float)pawn.Map.pathing.For(pawn).pathGrid.CalculatedCostAt(c, perceivedStatic: false, pawn.Position, pawnCellBaseCostOverride);
			Building edifice = c.GetEdifice(pawn.Map);
			if (edifice != null)
			{
				num += (float)(int)edifice.PathWalkCostFor(pawn);
			}
			TerrainDef terrain = c.GetTerrain(pawn.Map);
			if (terrain.tags != null)
			{
				foreach (string tag in terrain.tags)
				{
					if (pawn.kindDef.moveSpeedFactorByTerrainTag.TryGetValue(tag, out var value))
					{
						num /= value;
					}
				}
			}
			if (num > 450f)
			{
				num = 450f;
			}
			if (pawn.CurJob != null)
			{
				Pawn locomotionUrgencySameAs = pawn.jobs.curDriver.locomotionUrgencySameAs;
				if (locomotionUrgencySameAs != null && locomotionUrgencySameAs != pawn && locomotionUrgencySameAs.Spawned)
				{
					float num2 = CostToMoveIntoCell(locomotionUrgencySameAs, c);
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
						num *= 3f;
						if (num < 60f)
						{
							num = 60f;
						}
						break;
					case LocomotionUrgency.Walk:
						num *= 2f;
						if (num < 50f)
						{
							num = 50f;
						}
						break;
					case LocomotionUrgency.Jog:
						num *= 1f;
						break;
					case LocomotionUrgency.Sprint:
						num = Mathf.RoundToInt(num * 0.75f);
						break;
					}
				}
			}
			return Mathf.Max(num, 1f);
		}

		private float CostToPayThisTick()
		{
			float num = 1f;
			if (pawn.stances.stagger.Staggered)
			{
				num *= pawn.stances.stagger.StaggerMoveSpeedFactor;
			}
			if (pawn.Flying)
			{
				num *= pawn.RaceProps.flightSpeedFactor;
			}
			if (num < nextCellCostTotal / 450f)
			{
				num = nextCellCostTotal / 450f;
			}
			return num;
		}

		private void TrySetNewPathRequest()
		{
			if (curPathRequest == null)
			{
				SetNewPathRequest();
			}
		}

		private void SetNewPathRequest()
		{
			DisposeAndClearCurPathRequest();
			curPathRequest = GenerateNewPathRequest();
		}

		private PathRequest GenerateNewPathRequest()
		{
			if (debugLog)
			{
				Log.Message($"{pawn}: Created request for a new path to {destination} with {peMode} from {nextCell}");
			}
			cachedReturningToCell = GuestUtility.PrisonerCanReturnToCell(pawn);
			lastPathedTargetPosition = destination.Cell;
			PathFinder pathFinder = pawn.Map.pathFinder;
			PathRequest pathRequest = pathFinder.CreateRequest(tuning: PathFinderCostTuning.For(pawn), start: nextCell, target: destination, dest: null, pawn: pawn, peMode: peMode);
			pathFinder.PushRequest(pathRequest);
			return pathRequest;
		}

		public void DisposeAndClearCurPathRequest()
		{
			if (curPathRequest != null)
			{
				curPathRequest.Dispose();
			}
			curPathRequest = null;
		}

		public void DisposeAndClearCurPath()
		{
			if (curPath != null)
			{
				curPath.Dispose();
			}
			curPath = null;
		}

		private bool AtDestinationPosition()
		{
			return pawn.CanReachImmediate(destination, peMode);
		}

		private bool NeedNewPath()
		{
			if (destination.IsValid && pawn.CanReachImmediate(destination, peMode))
			{
				return false;
			}
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
			PathingContext pc = pawn.Map.pathing.For(pawn);
			bool canBashFences = pawn.CurJob != null && pawn.CurJob.canBashFences;
			IntVec3 other = IntVec3.Invalid;
			for (int i = 0; i < 20 && i < curPath.NodesLeftCount; i++)
			{
				IntVec3 intVec = curPath.Peek(i);
				if (!intVec.WalkableBy(pawn.Map, pawn))
				{
					return true;
				}
				if (!BestPathHadPawnsInTheWayRecently() && WillCollideWithPawnAt(intVec))
				{
					if (!ChangedCellRecently((int)nextCellCostTotal + 10))
					{
						ResetToCurrentPosition();
					}
					foundPathWhichCollidesWithPawns = GenTicks.TicksGame;
					return true;
				}
				if (!BestPathHadDangerRecently() && PawnUtility.KnownDangerAt(intVec, pawn.Map, pawn))
				{
					return true;
				}
				if (intVec.GetEdifice(pawn.Map) is Building_Door building_Door)
				{
					if (building_Door.BlocksPawn(pawn) && !building_Door.CanPhysicallyPass(pawn) && !pawn.HostileTo(building_Door))
					{
						return true;
					}
					if (building_Door.IsForbiddenToPass(pawn))
					{
						return true;
					}
				}
				if (i != 0 && intVec.AdjacentToDiagonal(other) && (PathUtility.BlocksDiagonalMovement(intVec.x, other.z, pc, canBashFences) || PathUtility.BlocksDiagonalMovement(other.x, intVec.z, pc, canBashFences)))
				{
					return true;
				}
				other = intVec;
			}
			return false;
		}

		public bool BestPathHadPawnsInTheWayRecently()
		{
			int num = (pawn.IsPlayerControlled ? 15 : 100);
			return foundPathWhichCollidesWithPawns + num > Find.TickManager.TicksGame;
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
