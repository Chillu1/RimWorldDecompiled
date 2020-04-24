using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanExitMapUtility
	{
		private static List<int> tmpNeighbors = new List<int>();

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<int> retTiles = new List<int>();

		private static readonly Rot4[] rotTmp = new Rot4[2];

		private static List<int> tileCandidates = new List<int>();

		public static Caravan ExitMapAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, Direction8Way dir, int destinationTile, bool sendMessage = true)
		{
			int directionTile = FindRandomStartingTileBasedOnExitDir(exitFromTile, dir);
			return ExitMapAndCreateCaravan(pawns, faction, exitFromTile, directionTile, destinationTile, sendMessage);
		}

		public static Caravan ExitMapAndCreateCaravan(IEnumerable<Pawn> pawns, Faction faction, int exitFromTile, int directionTile, int destinationTile, bool sendMessage = true)
		{
			if (!GenWorldClosest.TryFindClosestPassableTile(exitFromTile, out exitFromTile))
			{
				Log.Error("Could not find any passable tile for a new caravan.");
				return null;
			}
			if (Find.World.Impassable(directionTile))
			{
				directionTile = exitFromTile;
			}
			tmpPawns.Clear();
			tmpPawns.AddRange(pawns);
			Map map = null;
			for (int i = 0; i < tmpPawns.Count; i++)
			{
				AddCaravanExitTaleIfShould(tmpPawns[i]);
				map = tmpPawns[i].MapHeld;
				if (map != null)
				{
					break;
				}
			}
			Caravan caravan = CaravanMaker.MakeCaravan(tmpPawns, faction, exitFromTile, addToWorldPawnsIfNotAlready: false);
			Rot4 exitDir = (map != null) ? Find.WorldGrid.GetRotFromTo(exitFromTile, directionTile) : Rot4.Invalid;
			for (int j = 0; j < tmpPawns.Count; j++)
			{
				tmpPawns[j].ExitMap(allowedToJoinOrCreateCaravan: false, exitDir);
			}
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int k = 0; k < pawnsListForReading.Count; k++)
			{
				if (!pawnsListForReading[k].IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawnsListForReading[k]);
				}
			}
			if (map != null)
			{
				map.Parent.Notify_CaravanFormed(caravan);
				map.retainedCaravanData.Notify_CaravanFormed(caravan);
			}
			if (!caravan.pather.Moving && caravan.Tile != directionTile)
			{
				caravan.pather.StartPath(directionTile, null, repathImmediately: true);
				caravan.pather.nextTileCostLeft /= 2f;
				caravan.tweener.ResetTweenedPosToRoot();
			}
			if (destinationTile != -1)
			{
				List<FloatMenuOption> list = FloatMenuMakerWorld.ChoicesAtFor(destinationTile, caravan);
				if (list.Any((FloatMenuOption x) => !x.Disabled))
				{
					list.First((FloatMenuOption x) => !x.Disabled).action();
				}
				else
				{
					caravan.pather.StartPath(destinationTile, null, repathImmediately: true);
				}
			}
			if (sendMessage)
			{
				TaggedString taggedString = "MessageFormedCaravan".Translate(caravan.Name).CapitalizeFirst();
				if (caravan.pather.Moving && caravan.pather.ArrivalAction != null)
				{
					taggedString += " " + "MessageFormedCaravan_Orders".Translate() + ": " + caravan.pather.ArrivalAction.Label + ".";
				}
				Messages.Message(taggedString, caravan, MessageTypeDefOf.TaskCompletion);
			}
			return caravan;
		}

		public static void ExitMapAndJoinOrCreateCaravan(Pawn pawn, Rot4 exitDir)
		{
			Caravan caravan = FindCaravanToJoinFor(pawn);
			if (caravan != null)
			{
				AddCaravanExitTaleIfShould(pawn);
				caravan.AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny: true);
				pawn.ExitMap(allowedToJoinOrCreateCaravan: false, exitDir);
			}
			else if (pawn.IsColonist)
			{
				Map map = pawn.Map;
				int directionTile = FindRandomStartingTileBasedOnExitDir(map.Tile, exitDir);
				Caravan caravan2 = ExitMapAndCreateCaravan(Gen.YieldSingle(pawn), pawn.Faction, map.Tile, directionTile, -1, sendMessage: false);
				caravan2.autoJoinable = true;
				bool flag = false;
				List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (FindCaravanToJoinFor(allPawnsSpawned[i]) != null && !allPawnsSpawned[i].Downed && !allPawnsSpawned[i].Drafted)
					{
						if (allPawnsSpawned[i].RaceProps.Animal)
						{
							flag = true;
						}
						RestUtility.WakeUp(allPawnsSpawned[i]);
						allPawnsSpawned[i].jobs.CheckForJobOverride();
					}
				}
				TaggedString taggedString = "MessagePawnLeftMapAndCreatedCaravan".Translate(pawn.LabelShort, pawn).CapitalizeFirst();
				if (flag)
				{
					taggedString += " " + "MessagePawnLeftMapAndCreatedCaravan_AnimalsWantToJoin".Translate();
				}
				Messages.Message(taggedString, caravan2, MessageTypeDefOf.TaskCompletion);
			}
			else
			{
				Log.Error("Pawn " + pawn + " didn't find any caravan to join, and he can't create one.");
			}
		}

		public static bool CanExitMapAndJoinOrCreateCaravanNow(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return false;
			}
			if (!pawn.Map.exitMapGrid.MapUsesExitGrid)
			{
				return false;
			}
			if (!pawn.IsColonist)
			{
				return FindCaravanToJoinFor(pawn) != null;
			}
			return true;
		}

		public static List<int> AvailableExitTilesAt(Map map)
		{
			retTiles.Clear();
			int currentTileID = map.Tile;
			World world = Find.World;
			WorldGrid grid = world.grid;
			grid.GetTileNeighbors(currentTileID, tmpNeighbors);
			for (int i = 0; i < tmpNeighbors.Count; i++)
			{
				int num = tmpNeighbors[i];
				if (IsGoodCaravanStartingTile(num))
				{
					GetExitMapEdges(grid, currentTileID, num, out Rot4 primary, out Rot4 secondary);
					if (((primary != Rot4.Invalid && CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Walkable(map) && !x.Fogged(map), map, primary, CellFinder.EdgeRoadChance_Ignore, out IntVec3 result)) || (secondary != Rot4.Invalid && CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Walkable(map) && !x.Fogged(map), map, secondary, CellFinder.EdgeRoadChance_Ignore, out result))) && !retTiles.Contains(num))
					{
						retTiles.Add(num);
					}
				}
			}
			retTiles.SortBy((int x) => grid.GetHeadingFromTo(currentTileID, x));
			return retTiles;
		}

		public static void GetExitMapEdges(WorldGrid grid, int fromTileID, int toTileID, out Rot4 primary, out Rot4 secondary)
		{
			primary = (secondary = Rot4.Invalid);
			int num = 0;
			float heading = grid.GetHeadingFromTo(fromTileID, toTileID);
			if (heading >= 292.5f || heading <= 67.5f)
			{
				rotTmp[num++] = Rot4.North;
			}
			else if (heading >= 112.5f && heading <= 247.5f)
			{
				rotTmp[num++] = Rot4.South;
			}
			if (heading >= 22.5f && heading <= 157.5f)
			{
				rotTmp[num++] = Rot4.East;
			}
			else if (heading >= 202.5f && heading <= 337.5f)
			{
				rotTmp[num++] = Rot4.West;
			}
			Array.Sort(rotTmp, (Rot4 r1, Rot4 r2) => Mathf.Abs(r1.AsAngle - heading).CompareTo(Mathf.Abs(r2.AsAngle - heading)));
			if (num >= 1)
			{
				primary = rotTmp[0];
			}
			if (num >= 2)
			{
				secondary = rotTmp[1];
			}
		}

		public static int RandomBestExitTileFrom(Map map)
		{
			Tile tileInfo = map.TileInfo;
			List<int> options = AvailableExitTilesAt(map);
			if (!options.Any())
			{
				return -1;
			}
			List<Tile.RoadLink> roads = tileInfo.Roads;
			if (roads == null)
			{
				return options.RandomElement();
			}
			int bestRoadIndex = -1;
			for (int i = 0; i < roads.Count; i++)
			{
				if (options.Contains(roads[i].neighbor) && (bestRoadIndex == -1 || roads[i].road.priority > roads[bestRoadIndex].road.priority))
				{
					bestRoadIndex = i;
				}
			}
			if (bestRoadIndex == -1)
			{
				return options.RandomElement();
			}
			return roads.Where((Tile.RoadLink rl) => options.Contains(rl.neighbor) && rl.road == roads[bestRoadIndex].road).RandomElement().neighbor;
		}

		public static int BestExitTileToGoTo(int destinationTile, Map from)
		{
			int num = -1;
			using (WorldPath worldPath = Find.WorldPathFinder.FindPath(from.Tile, destinationTile, null))
			{
				if (worldPath.Found && worldPath.NodesLeftCount >= 2)
				{
					num = worldPath.NodesReversed[worldPath.NodesReversed.Count - 2];
				}
			}
			if (num == -1)
			{
				return RandomBestExitTileFrom(from);
			}
			float num2 = 0f;
			int num3 = -1;
			List<int> list = AvailableExitTilesAt(from);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == num)
				{
					return list[i];
				}
				float num4 = (Find.WorldGrid.GetTileCenter(list[i]) - Find.WorldGrid.GetTileCenter(num)).MagnitudeHorizontalSquared();
				if (num3 == -1 || num4 < num2)
				{
					num3 = list[i];
					num2 = num4;
				}
			}
			return num3;
		}

		private static int FindRandomStartingTileBasedOnExitDir(int tileID, Rot4 exitDir)
		{
			tileCandidates.Clear();
			World world = Find.World;
			WorldGrid grid = world.grid;
			grid.GetTileNeighbors(tileID, tmpNeighbors);
			for (int i = 0; i < tmpNeighbors.Count; i++)
			{
				int num = tmpNeighbors[i];
				if (IsGoodCaravanStartingTile(num) && (!exitDir.IsValid || !(grid.GetRotFromTo(tileID, num) != exitDir)))
				{
					tileCandidates.Add(num);
				}
			}
			if (tileCandidates.TryRandomElement(out int result))
			{
				return result;
			}
			if (tmpNeighbors.Where(delegate(int x)
			{
				if (!IsGoodCaravanStartingTile(x))
				{
					return false;
				}
				Rot4 rotFromTo = grid.GetRotFromTo(tileID, x);
				return ((exitDir == Rot4.North || exitDir == Rot4.South) && (rotFromTo == Rot4.East || rotFromTo == Rot4.West)) || ((exitDir == Rot4.East || exitDir == Rot4.West) && (rotFromTo == Rot4.North || rotFromTo == Rot4.South));
			}).TryRandomElement(out result))
			{
				return result;
			}
			if (tmpNeighbors.Where((int x) => IsGoodCaravanStartingTile(x)).TryRandomElement(out result))
			{
				return result;
			}
			return tileID;
		}

		private static int FindRandomStartingTileBasedOnExitDir(int tileID, Direction8Way exitDir)
		{
			tileCandidates.Clear();
			WorldGrid grid = Find.World.grid;
			grid.GetTileNeighbors(tileID, tmpNeighbors);
			for (int i = 0; i < tmpNeighbors.Count; i++)
			{
				int num = tmpNeighbors[i];
				if (IsGoodCaravanStartingTile(num) && grid.GetDirection8WayFromTo(tileID, num) == exitDir)
				{
					tileCandidates.Add(num);
				}
			}
			if (tileCandidates.TryRandomElement(out int result))
			{
				return result;
			}
			if (tmpNeighbors.Where((int x) => IsGoodCaravanStartingTile(x)).TryRandomElement(out result))
			{
				return result;
			}
			return tileID;
		}

		private static bool IsGoodCaravanStartingTile(int tile)
		{
			return !Find.World.Impassable(tile);
		}

		public static Caravan FindCaravanToJoinFor(Pawn pawn)
		{
			if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer)
			{
				return null;
			}
			if (!pawn.Spawned || !pawn.CanReachMapEdge())
			{
				return null;
			}
			int tile = pawn.Map.Tile;
			Find.WorldGrid.GetTileNeighbors(tile, tmpNeighbors);
			tmpNeighbors.Add(tile);
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan = caravans[i];
				if (!tmpNeighbors.Contains(caravan.Tile) || !caravan.autoJoinable)
				{
					continue;
				}
				if (pawn.HostFaction == null)
				{
					if (caravan.Faction == pawn.Faction)
					{
						return caravan;
					}
				}
				else if (caravan.Faction == pawn.HostFaction)
				{
					return caravan;
				}
			}
			return null;
		}

		public static bool AnyoneTryingToJoinCaravan(Caravan c)
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome || !Find.WorldGrid.IsNeighborOrSame(c.Tile, map.Tile))
				{
					continue;
				}
				List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
				for (int j = 0; j < allPawnsSpawned.Count; j++)
				{
					if (!allPawnsSpawned[j].IsColonistPlayerControlled && !allPawnsSpawned[j].Downed && FindCaravanToJoinFor(allPawnsSpawned[j]) == c)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static void OpenSomeoneTryingToJoinCaravanDialog(Caravan c, Action confirmAction)
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmMoveAutoJoinableCaravan".Translate(), confirmAction));
		}

		private static void AddCaravanExitTaleIfShould(Pawn pawn)
		{
			if (pawn.Spawned && pawn.IsFreeColonist)
			{
				if (pawn.Map.IsPlayerHome)
				{
					TaleRecorder.RecordTale(TaleDefOf.CaravanFormed, pawn);
				}
				else if (GenHostility.AnyHostileActiveThreatToPlayer(pawn.Map))
				{
					TaleRecorder.RecordTale(TaleDefOf.CaravanFled, pawn);
				}
			}
		}
	}
}
