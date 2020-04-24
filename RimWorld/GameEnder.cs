using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class GameEnder : IExposable
	{
		public bool gameEnding;

		private int ticksToGameOver = -1;

		private const int GameEndCountdownDuration = 400;

		public void ExposeData()
		{
			Scribe_Values.Look(ref gameEnding, "gameEnding", defaultValue: false);
			Scribe_Values.Look(ref ticksToGameOver, "ticksToGameOver", -1);
		}

		public void CheckOrUpdateGameOver()
		{
			if (Find.TickManager.TicksGame < 300)
			{
				return;
			}
			if (ShipCountdown.CountingDown)
			{
				gameEnding = false;
				return;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (maps[i].mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount >= 1)
				{
					gameEnding = false;
					return;
				}
			}
			for (int j = 0; j < maps.Count; j++)
			{
				List<Pawn> allPawnsSpawned = maps[j].mapPawns.AllPawnsSpawned;
				for (int k = 0; k < allPawnsSpawned.Count; k++)
				{
					if (allPawnsSpawned[k].carryTracker != null)
					{
						Pawn pawn = allPawnsSpawned[k].carryTracker.CarriedThing as Pawn;
						if (pawn != null && pawn.IsFreeColonist)
						{
							gameEnding = false;
							return;
						}
					}
				}
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int l = 0; l < caravans.Count; l++)
			{
				if (IsPlayerControlledWithFreeColonist(caravans[l]))
				{
					gameEnding = false;
					return;
				}
			}
			List<TravelingTransportPods> travelingTransportPods = Find.WorldObjects.TravelingTransportPods;
			for (int m = 0; m < travelingTransportPods.Count; m++)
			{
				if (travelingTransportPods[m].PodsHaveAnyFreeColonist)
				{
					gameEnding = false;
					return;
				}
			}
			if (QuestUtility.TotalBorrowedColonistCount() <= 0 && !gameEnding)
			{
				gameEnding = true;
				ticksToGameOver = 400;
			}
		}

		public void GameEndTick()
		{
			if (gameEnding)
			{
				ticksToGameOver--;
				if (ticksToGameOver == 0)
				{
					GenGameEnd.EndGameDialogMessage("GameOverEveryoneDead".Translate());
				}
			}
		}

		private bool IsPlayerControlledWithFreeColonist(Caravan caravan)
		{
			if (!caravan.IsPlayerControlled)
			{
				return false;
			}
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Pawn pawn = pawnsListForReading[i];
				if (pawn.IsColonist && pawn.HostFaction == null)
				{
					return true;
				}
			}
			return false;
		}
	}
}
