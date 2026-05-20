using System.Text;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet;

public static class SettleInEmptyTileUtility
{
	private const int MinStartingLocCellsCount = 600;

	private static StringBuilder tmpSettleFailReason = new StringBuilder();

	public static void Settle(Caravan caravan)
	{
		Faction faction = caravan.Faction;
		if (faction != Faction.OfPlayer)
		{
			Log.Error("Cannot settle with non-player faction.");
			return;
		}
		if (Find.AnyPlayerHomeMap == null)
		{
			foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
			{
				MemoryThoughtHandler memoryThoughtHandler = allMapsCaravansAndTravellingTransporters_Alive_Colonist.needs?.mood?.thoughts?.memories;
				if (memoryThoughtHandler != null)
				{
					memoryThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.NewColonyOptimism);
					memoryThoughtHandler.RemoveMemoriesOfDef(ThoughtDefOf.NewColonyHope);
					if (allMapsCaravansAndTravellingTransporters_Alive_Colonist.IsFreeNonSlaveColonist)
					{
						memoryThoughtHandler.TryGainMemory(ThoughtDefOf.NewColonyOptimism);
					}
				}
			}
		}
		Settlement newHome = SettleUtility.AddNewHome(caravan.Tile, faction);
		LongEventHandler.QueueLongEvent(delegate
		{
			GetOrGenerateMapUtility.GetOrGenerateMap(caravan.Tile, Find.World.info.initialMapSize, null);
		}, "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
		LongEventHandler.QueueLongEvent(delegate
		{
			Map map = newHome.Map;
			Pawn pawn = caravan.PawnsListForReading[0];
			CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Center, CaravanDropInventoryMode.DropInstantly, draftColonists: false, (IntVec3 x) => x.GetRoom(map).CellCount >= 600);
			newHome.Notify_MyMapSettled(map);
			CameraJumper.TryJump(pawn);
		}, "SpawningColonists", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
	}

	public static Command SettleCommand(Caravan caravan)
	{
		Command_Settle command_Settle = new Command_Settle();
		command_Settle.defaultLabel = "CommandSettle".Translate();
		command_Settle.defaultDesc = "CommandSettleDesc".Translate();
		command_Settle.icon = SettleUtility.SettleCommandTex;
		command_Settle.action = delegate
		{
			SoundDefOf.Tick_High.PlayOneShotOnCamera();
			SettlementProximityGoodwillUtility.CheckConfirmSettle(caravan.Tile, delegate
			{
				Settle(caravan);
			});
		};
		tmpSettleFailReason.Length = 0;
		if (!TileFinder.IsValidTileForNewSettlement(caravan.Tile, tmpSettleFailReason))
		{
			command_Settle.Disable(tmpSettleFailReason.ToString());
		}
		else if (SettleUtility.PlayerSettlementsCountLimitReached)
		{
			if (Prefs.MaxNumberOfPlayerSettlements > 1)
			{
				command_Settle.Disable("CommandSettleFailReachedMaximumNumberOfBases".Translate());
			}
			else
			{
				command_Settle.Disable("CommandSettleFailAlreadyHaveBase".Translate());
			}
		}
		return command_Settle;
	}

	public static Command SetupCamp(Caravan caravan)
	{
		Command_Action command_Action = new Command_Action();
		command_Action.defaultLabel = "CommandCamp".Translate();
		command_Action.defaultDesc = "CommandCampDesc".Translate();
		command_Action.icon = SettleUtility.CreateCampCommandTex;
		command_Action.action = delegate
		{
			LongEventHandler.QueueLongEvent(delegate
			{
				Map map = GetOrGenerateMapUtility.GetOrGenerateMap(caravan.Tile, WorldObjectDefOf.Camp.overrideMapSize ?? Find.World.info.initialMapSize, WorldObjectDefOf.Camp);
				map.Parent.SetFaction(caravan.Faction);
				Pawn pawn = caravan.PawnsListForReading[0];
				CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Center, CaravanDropInventoryMode.DoNotDrop, draftColonists: false, delegate(IntVec3 x)
				{
					if (x.GetRoom(map).CellCount < 600)
					{
						return false;
					}
					return !x.GetTerrain(map).IsWater;
				});
				map.Parent.GetComponent<TimedDetectionRaids>()?.StartDetectionCountdown(240000, 60000);
				CameraJumper.TryJump(pawn);
			}, "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
		};
		if (!CanCreateMapAt(caravan.Tile) || Find.WorldObjects.AnyMapParentAt(caravan.Tile))
		{
			command_Action.Disable("CommandCampFailExistingWorldObject".Translate());
		}
		return command_Action;
	}

	public static bool CanCreateMapAt(PlanetTile tile, bool forGravship = false)
	{
		foreach (WorldObject item in Find.WorldObjects.ObjectsAt(tile))
		{
			if (!item.def.canHaveMap)
			{
				return false;
			}
		}
		if (Find.WorldObjects.AnyMapParentAt(tile) || Current.Game.FindMap(tile) != null)
		{
			return false;
		}
		return true;
	}
}
