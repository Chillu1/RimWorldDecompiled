using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public sealed class GameEnder : IExposable
{
	public bool gameEnding;

	private int ticksToGameOver = -1;

	public int newWanderersCreatedTick = -99999;

	private const int GameEndCountdownDuration = 400;

	public const int NewWanderersDelay = 20000;

	public const int NewWanderersRespiteDays = 5;

	public void ExposeData()
	{
		Scribe_Values.Look(ref gameEnding, "gameEnding", defaultValue: false);
		Scribe_Values.Look(ref ticksToGameOver, "ticksToGameOver", -1);
		Scribe_Values.Look(ref newWanderersCreatedTick, "newWanderersCreatedTick", -99999);
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
		if (ModsConfig.OdysseyActive && WorldComponent_GravshipController.CutsceneInProgress)
		{
			gameEnding = false;
			return;
		}
		if (Find.CurrentGravship != null)
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
			IReadOnlyList<Pawn> allPawnsSpawned = maps[j].mapPawns.AllPawnsSpawned;
			for (int k = 0; k < allPawnsSpawned.Count; k++)
			{
				if (allPawnsSpawned[k].carryTracker != null && allPawnsSpawned[k].carryTracker.CarriedThing is Pawn { IsFreeColonist: not false })
				{
					gameEnding = false;
					return;
				}
			}
		}
		if (ModsConfig.AnomalyActive && DeathRefusalUtility.PlayerHasCorpseWithDeathRefusal())
		{
			gameEnding = false;
			return;
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
		List<TravellingTransporters> travellingTransporters = Find.WorldObjects.TravellingTransporters;
		for (int m = 0; m < travellingTransporters.Count; m++)
		{
			if (travellingTransporters[m].PodsHaveAnyFreeColonist)
			{
				gameEnding = false;
				return;
			}
		}
		if (QuestUtility.TotalBorrowedColonistCount() > 0)
		{
			return;
		}
		if (ModsConfig.AnomalyActive)
		{
			for (int n = 0; n < maps.Count; n++)
			{
				List<Pawn> allPawnsUnspawned = maps[n].mapPawns.AllPawnsUnspawned;
				for (int num = 0; num < allPawnsUnspawned.Count; num++)
				{
					Pawn pawn2 = allPawnsUnspawned[num];
					if (pawn2.IsColonist && pawn2.HostFaction == null && pawn2.ParentHolder is CompDevourer)
					{
						gameEnding = false;
						return;
					}
				}
			}
		}
		if (!gameEnding)
		{
			gameEnding = true;
			ticksToGameOver = 400;
		}
	}

	public bool CanSpawnNewWanderers()
	{
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction)
		{
			if (item.RaceProps.Humanlike && !item.Dead)
			{
				return false;
			}
			if (ModsConfig.AnomalyActive && (DeathRefusalUtility.HasPlayerControlledDeathRefusal(item) || DeathRefusalUtility.PlayerHasCorpseWithDeathRefusal()))
			{
				return false;
			}
		}
		if (ModsConfig.AnomalyActive && Find.Anomaly.VoidAwakeningActive())
		{
			return false;
		}
		return true;
	}

	public void GameEndTick()
	{
		if (gameEnding)
		{
			ticksToGameOver--;
			if (ticksToGameOver == 0 && Find.LetterStack.LettersListForReading.Find((Letter letter3) => letter3.def == LetterDefOf.GameEnded) == null)
			{
				Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter("GameOver".Translate(), "GameOverEveryoneDead".Translate(), LetterDefOf.GameEnded));
			}
			if (Mathf.Abs(ticksToGameOver) == 20000 && CanSpawnNewWanderers())
			{
				Letter letter = Find.LetterStack.LettersListForReading.Find((Letter letter3) => letter3.def == LetterDefOf.GameEnded);
				ChoiceLetter choiceLetter = LetterMaker.MakeLetter("GameOverCreateNewWanderers".Translate(), "GameOverCreateNewWanderersText".Translate(), LetterDefOf.GameEnded);
				Find.LetterStack.ReceiveLetter(choiceLetter);
				Find.LetterStack.RemoveLetter(letter);
				choiceLetter.arrivalTick = letter.arrivalTick;
			}
		}
		else
		{
			Letter letter2 = Find.LetterStack.LettersListForReading.Find((Letter let) => let.def == LetterDefOf.GameEnded);
			if (letter2 != null)
			{
				Find.LetterStack.RemoveLetter(letter2);
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
