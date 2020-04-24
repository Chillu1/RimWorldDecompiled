using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
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
		}

		private static void CountdownEnded()
		{
			if (shipRoot != null)
			{
				List<Building> list = ShipUtility.ShipBuildingsAttachedTo(shipRoot).ToList();
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Building item in list)
				{
					Building_CryptosleepCasket building_CryptosleepCasket = item as Building_CryptosleepCasket;
					if (building_CryptosleepCasket != null && building_CryptosleepCasket.HasAnyContents)
					{
						stringBuilder.AppendLine("   " + building_CryptosleepCasket.ContainedThing.LabelCap);
						Find.StoryWatcher.statsRecord.colonistsLaunched++;
						TaleRecorder.RecordTale(TaleDefOf.LaunchedShip, building_CryptosleepCasket.ContainedThing);
					}
				}
				GameVictoryUtility.ShowCredits(GameVictoryUtility.MakeEndCredits("GameOverShipLaunchedIntro".Translate(), "GameOverShipLaunchedEnding".Translate(), stringBuilder.ToString()));
				foreach (Building item2 in list)
				{
					item2.Destroy();
				}
			}
			else if (!customLaunchString.NullOrEmpty())
			{
				GameVictoryUtility.ShowCredits(customLaunchString);
			}
			else
			{
				GameVictoryUtility.ShowCredits(GameVictoryUtility.MakeEndCredits("GameOverShipLaunchedIntro".Translate(), "GameOverShipLaunchedEnding".Translate(), null));
			}
		}
	}
}
