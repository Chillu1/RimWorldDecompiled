using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Caravan_PathFollower : IExposable
	{
		private Caravan caravan;

		private bool moving;

		private bool paused;

		public int nextTile = -1;

		public int previousTileForDrawingIfInDoubt = -1;

		public float nextTileCostLeft;

		public float nextTileCostTotal = 1f;

		private int destTile;

		private CaravanArrivalAction arrivalAction;

		public WorldPath curPath;

		public int lastPathedTargetTile;

		public const int MaxMoveTicks = 30000;

		private const int MaxCheckAheadNodes = 20;

		private const int MinCostWalk = 50;

		private const int MinCostAmble = 60;

		public const float DefaultPathCostToPayPerTick = 1f;

		public const int FinalNoRestPushMaxDurationTicks = 10000;

		public int Destination => destTile;

		public bool Moving
		{
			get
			{
				if (moving)
				{
					return caravan.Spawned;
				}
				return false;
			}
		}

		public bool MovingNow
		{
			get
			{
				if (Moving && !Paused)
				{
					return !caravan.CantMove;
				}
				return false;
			}
		}

		public CaravanArrivalAction ArrivalAction
		{
			get
			{
				if (!Moving)
				{
					return null;
				}
				return arrivalAction;
			}
		}

		public bool Paused
		{
			get
			{
				if (Moving)
				{
					return paused;
				}
				return false;
			}
			set
			{
				if (value != paused)
				{
					if (!value)
					{
						paused = false;
					}
					else if (!Moving)
					{
						Log.Error("Tried to pause caravan movement of " + caravan.ToStringSafe() + " but it's not moving.");
					}
					else
					{
						paused = true;
					}
					caravan.Notify_DestinationOrPauseStatusChanged();
				}
			}
		}

		public Caravan_PathFollower(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref moving, "moving", defaultValue: true);
			Scribe_Values.Look(ref paused, "paused", defaultValue: false);
			Scribe_Values.Look(ref nextTile, "nextTile", 0);
			Scribe_Values.Look(ref previousTileForDrawingIfInDoubt, "previousTileForDrawingIfInDoubt", 0);
			Scribe_Values.Look(ref nextTileCostLeft, "nextTileCostLeft", 0f);
			Scribe_Values.Look(ref nextTileCostTotal, "nextTileCostTotal", 0f);
			Scribe_Values.Look(ref destTile, "destTile", 0);
			Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && Current.ProgramState != 0 && moving && !StartPath(destTile, arrivalAction, repathImmediately: true, resetPauseStatus: false))
			{
				StopDead();
			}
		}

		public bool StartPath(int destTile, CaravanArrivalAction arrivalAction, bool repathImmediately = false, bool resetPauseStatus = true)
		{
			caravan.autoJoinable = false;
			if (resetPauseStatus)
			{
				paused = false;
			}
			if (arrivalAction != null && !arrivalAction.StillValid(caravan, destTile))
			{
				return false;
			}
			if (!IsPassable(caravan.Tile) && !TryRecoverFromUnwalkablePosition())
			{
				return false;
			}
			if (moving && curPath != null && this.destTile == destTile)
			{
				this.arrivalAction = arrivalAction;
				return true;
			}
			if (!caravan.CanReach(destTile))
			{
				PatherFailed();
				return false;
			}
			this.destTile = destTile;
			this.arrivalAction = arrivalAction;
			caravan.Notify_DestinationOrPauseStatusChanged();
			if (nextTile < 0 || !IsNextTilePassable())
			{
				nextTile = caravan.Tile;
				nextTileCostLeft = 0f;
				previousTileForDrawingIfInDoubt = -1;
			}
			if (AtDestinationPosition())
			{
				PatherArrived();
				return true;
			}
			if (curPath != null)
			{
				curPath.ReleaseToPool();
			}
			curPath = null;
			moving = true;
			if (repathImmediately && TrySetNewPath() && nextTileCostLeft <= 0f && moving)
			{
				TryEnterNextPathTile();
			}
			return true;
		}

		public void StopDead()
		{
			if (curPath != null)
			{
				curPath.ReleaseToPool();
			}
			curPath = null;
			moving = false;
			paused = false;
			nextTile = caravan.Tile;
			previousTileForDrawingIfInDoubt = -1;
			arrivalAction = null;
			nextTileCostLeft = 0f;
			caravan.Notify_DestinationOrPauseStatusChanged();
		}

		public void PatherTick()
		{
			if (moving && arrivalAction != null && !arrivalAction.StillValid(caravan, Destination))
			{
				string failMessage = arrivalAction.StillValid(caravan, Destination).FailMessage;
				Messages.Message("MessageCaravanArrivalActionNoLongerValid".Translate(caravan.Name).CapitalizeFirst() + ((failMessage != null) ? (" " + failMessage) : ""), caravan, MessageTypeDefOf.NegativeEvent);
				StopDead();
			}
			if (!caravan.CantMove && !paused)
			{
				if (nextTileCostLeft > 0f)
				{
					nextTileCostLeft -= CostToPayThisTick();
				}
				else if (moving)
				{
					TryEnterNextPathTile();
				}
			}
		}

		public void Notify_Teleported_Int()
		{
			StopDead();
		}

		private bool IsPassable(int tile)
		{
			return !Find.World.Impassable(tile);
		}

		public bool IsNextTilePassable()
		{
			return IsPassable(nextTile);
		}

		private bool TryRecoverFromUnwalkablePosition()
		{
			if (GenWorldClosest.TryFindClosestTile(caravan.Tile, (int t) => IsPassable(t), out int foundTile))
			{
				Log.Warning(caravan + " on unwalkable tile " + caravan.Tile + ". Teleporting to " + foundTile);
				caravan.Tile = foundTile;
				caravan.Notify_Teleported();
				return true;
			}
			Log.Error(caravan + " on unwalkable tile " + caravan.Tile + ". Could not find walkable position nearby. Removed.");
			caravan.Destroy();
			return false;
		}

		private void PatherArrived()
		{
			CaravanArrivalAction caravanArrivalAction = arrivalAction;
			StopDead();
			if (caravanArrivalAction != null && (bool)caravanArrivalAction.StillValid(caravan, caravan.Tile))
			{
				caravanArrivalAction.Arrived(caravan);
			}
			else if (caravan.IsPlayerControlled && !caravan.VisibleToCameraNow())
			{
				Messages.Message("MessageCaravanArrivedAtDestination".Translate(caravan.Label), caravan, MessageTypeDefOf.TaskCompletion);
			}
		}

		private void PatherFailed()
		{
			StopDead();
		}

		private void TryEnterNextPathTile()
		{
			if (!IsNextTilePassable())
			{
				PatherFailed();
				return;
			}
			caravan.Tile = nextTile;
			if (!NeedNewPath() || TrySetNewPath())
			{
				if (AtDestinationPosition())
				{
					PatherArrived();
				}
				else if (curPath.NodesLeftCount == 0)
				{
					Log.Error(caravan + " ran out of path nodes. Force-arriving.");
					PatherArrived();
				}
				else
				{
					SetupMoveIntoNextTile();
				}
			}
		}

		private void SetupMoveIntoNextTile()
		{
			if (curPath.NodesLeftCount < 2)
			{
				Log.Error(caravan + " at " + caravan.Tile + " ran out of path nodes while pathing to " + destTile + ".");
				PatherFailed();
				return;
			}
			nextTile = curPath.ConsumeNextNode();
			previousTileForDrawingIfInDoubt = -1;
			if (Find.World.Impassable(nextTile))
			{
				Log.Error(caravan + " entering " + nextTile + " which is unwalkable.");
			}
			int num = CostToMove(caravan.Tile, nextTile);
			nextTileCostTotal = num;
			nextTileCostLeft = num;
		}

		private int CostToMove(int start, int end)
		{
			return CostToMove(caravan, start, end);
		}

		public static int CostToMove(Caravan caravan, int start, int end, int? ticksAbs = null)
		{
			return CostToMove(caravan.TicksPerMove, start, end, ticksAbs);
		}

		public static int CostToMove(int caravanTicksPerMove, int start, int end, int? ticksAbs = null, bool perceivedStatic = false, StringBuilder explanation = null, string caravanTicksPerMoveExplanation = null)
		{
			if (start == end)
			{
				return 0;
			}
			if (explanation != null)
			{
				explanation.Append(caravanTicksPerMoveExplanation);
				explanation.AppendLine();
			}
			StringBuilder stringBuilder = (explanation != null) ? new StringBuilder() : null;
			float num = (!perceivedStatic || explanation != null) ? WorldPathGrid.CalculatedMovementDifficultyAt(end, perceivedStatic, ticksAbs, stringBuilder) : Find.WorldPathGrid.PerceivedMovementDifficultyAt(end);
			float roadMovementDifficultyMultiplier = Find.WorldGrid.GetRoadMovementDifficultyMultiplier(start, end, stringBuilder);
			if (explanation != null)
			{
				explanation.AppendLine();
				explanation.Append("TileMovementDifficulty".Translate() + ":");
				explanation.AppendLine();
				explanation.Append(stringBuilder.ToString().Indented("  "));
				explanation.AppendLine();
				explanation.Append("  = " + (num * roadMovementDifficultyMultiplier).ToString("0.#"));
			}
			int value = (int)((float)caravanTicksPerMove * num * roadMovementDifficultyMultiplier);
			value = Mathf.Clamp(value, 1, 30000);
			if (explanation != null)
			{
				explanation.AppendLine();
				explanation.AppendLine();
				explanation.Append("FinalCaravanMovementSpeed".Translate() + ":");
				int num2 = Mathf.CeilToInt((float)value / 1f);
				explanation.AppendLine();
				explanation.Append("  " + (60000f / (float)caravanTicksPerMove).ToString("0.#") + " / " + (num * roadMovementDifficultyMultiplier).ToString("0.#") + " = " + (60000f / (float)num2).ToString("0.#") + " " + "TilesPerDay".Translate());
			}
			return value;
		}

		public static bool IsValidFinalPushDestination(int tile)
		{
			List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < allWorldObjects.Count; i++)
			{
				if (allWorldObjects[i].Tile == tile && !(allWorldObjects[i] is Caravan))
				{
					return true;
				}
			}
			return false;
		}

		private float CostToPayThisTick()
		{
			float num = 1f;
			if (DebugSettings.fastCaravans)
			{
				num = 100f;
			}
			if (num < nextTileCostTotal / 30000f)
			{
				num = nextTileCostTotal / 30000f;
			}
			return num;
		}

		private bool TrySetNewPath()
		{
			WorldPath worldPath = GenerateNewPath();
			if (!worldPath.Found)
			{
				PatherFailed();
				return false;
			}
			if (curPath != null)
			{
				curPath.ReleaseToPool();
			}
			curPath = worldPath;
			return true;
		}

		private WorldPath GenerateNewPath()
		{
			int num = (moving && nextTile >= 0 && IsNextTilePassable()) ? nextTile : caravan.Tile;
			lastPathedTargetTile = destTile;
			WorldPath worldPath = Find.WorldPathFinder.FindPath(num, destTile, caravan);
			if (worldPath.Found && num != caravan.Tile)
			{
				if (worldPath.NodesLeftCount >= 2 && worldPath.Peek(1) == caravan.Tile)
				{
					worldPath.ConsumeNextNode();
					if (moving)
					{
						previousTileForDrawingIfInDoubt = nextTile;
						nextTile = caravan.Tile;
						nextTileCostLeft = nextTileCostTotal - nextTileCostLeft;
					}
				}
				else
				{
					worldPath.AddNodeAtStart(caravan.Tile);
				}
			}
			return worldPath;
		}

		private bool AtDestinationPosition()
		{
			return caravan.Tile == destTile;
		}

		private bool NeedNewPath()
		{
			if (!moving)
			{
				return false;
			}
			if (curPath == null || !curPath.Found || curPath.NodesLeftCount == 0)
			{
				return true;
			}
			for (int i = 0; i < 20 && i < curPath.NodesLeftCount; i++)
			{
				int tileID = curPath.Peek(i);
				if (Find.World.Impassable(tileID))
				{
					return true;
				}
			}
			return false;
		}
	}
}
