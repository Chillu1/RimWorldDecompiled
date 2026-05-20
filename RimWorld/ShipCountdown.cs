using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public static class ShipCountdown
{
	private static float timeLeft = -1000f;

	private static Building shipRoot;

	private static string customLaunchString;

	private const float InitialTime = 7.2f;

	public static bool CountingDown => timeLeft >= 0f;

	public static void InitiateCountdown(Building launchingShipRoot)
	{
		SoundDefOf.ShipTakeoff.PlayOneShotOnCamera();
		shipRoot = launchingShipRoot;
		timeLeft = 7.2f;
		customLaunchString = null;
		ScreenFader.StartFade(Color.white, 7.2f);
	}

	public static void InitiateCountdown(string launchString)
	{
		shipRoot = null;
		timeLeft = 7.2f;
		customLaunchString = launchString;
		ScreenFader.StartFade(Color.white, 7.2f);
	}

	public static void ShipCountdownUpdate()
	{
		if (timeLeft > 0f)
		{
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
			{
				CountdownEnded();
			}
		}
	}

	public static void CancelCountdown()
	{
		timeLeft = -1000f;
		ScreenFader.SetColor(Color.clear);
	}

	private static void CountdownEnded()
	{
		if (shipRoot != null)
		{
			TaggedString taggedString = "GameOverShipLaunchedEnding".Translate();
			List<Building> list = ShipUtility.ShipBuildingsAttachedTo(shipRoot).ToList();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Building item in list)
			{
				if (item is Building_CryptosleepCasket { HasAnyContents: not false } building_CryptosleepCasket)
				{
					stringBuilder.AppendLine("   " + building_CryptosleepCasket.ContainedThing.LabelCap);
					Find.StoryWatcher.statsRecord.colonistsLaunched++;
					TaleRecorder.RecordTale(TaleDefOf.LaunchedShip, building_CryptosleepCasket.ContainedThing);
				}
			}
			GameVictoryUtility.ShowCredits(GameVictoryUtility.MakeEndCredits("GameOverShipLaunchedIntro".Translate(), taggedString, stringBuilder.ToString()), SongDefOf.EndCreditsSong);
			{
				foreach (Building item2 in list)
				{
					item2.Destroy();
				}
				return;
			}
		}
		if (!customLaunchString.NullOrEmpty())
		{
			GameVictoryUtility.ShowCredits(customLaunchString, SongDefOf.EndCreditsSong);
		}
		else
		{
			GameVictoryUtility.ShowCredits(GameVictoryUtility.MakeEndCredits("GameOverShipLaunchedIntro".Translate(), "GameOverShipLaunchedEnding".Translate(), null), SongDefOf.EndCreditsSong);
		}
	}
}
