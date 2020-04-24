using System.Text;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
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
			Settlement newHome = SettleUtility.AddNewHome(caravan.Tile, faction);
			LongEventHandler.QueueLongEvent(delegate
			{
				GetOrGenerateMapUtility.GetOrGenerateMap(caravan.Tile, Find.World.info.initialMapSize, null);
			}, "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
			LongEventHandler.QueueLongEvent(delegate
			{
				Map map = newHome.Map;
				Pawn t = caravan.PawnsListForReading[0];
				CaravanEnterMapUtility.Enter(caravan, map, CaravanEnterMode.Center, CaravanDropInventoryMode.DropInstantly, draftColonists: false, (IntVec3 x) => x.GetRoom(map).CellCount >= 600);
				CameraJumper.TryJump(t);
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
	}
}
