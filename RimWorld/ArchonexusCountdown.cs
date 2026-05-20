using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class ArchonexusCountdown
	{
		private const float ScreenFadeSeconds = 6f;

		private const float SongStartDelay = 2.5f;

		private static float timeLeft = -1f;

		private static Building_ArchonexusCore archonexusCoreRoot;

		public static bool CountdownActivated => timeLeft > 0f;

		public static void InitiateCountdown(Building_ArchonexusCore archonexusCore)
		{
			if (ModLister.CheckIdeology("Archonexus victory countdown"))
			{
				archonexusCoreRoot = archonexusCore;
				timeLeft = 6f;
				SoundDefOf.Archotech_Invoked.PlayOneShot(archonexusCore);
				ScreenFader.StartFade(Color.white, 6f);
			}
		}

		public static void CancelCountdown()
		{
			timeLeft = -1f;
			archonexusCoreRoot = null;
			ScreenFader.SetColor(Color.clear);
		}

		private static void EndGame()
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<Pawn> list = (from p in archonexusCoreRoot.Map.mapPawns.PawnsInFaction(Faction.OfPlayer)
				where p.RaceProps.Humanlike
				select p).ToList();
			foreach (Pawn item in list)
			{
				if (!item.Dead && !item.IsQuestLodger())
				{
					stringBuilder.AppendLine("   " + item.LabelCap);
					Find.StoryWatcher.statsRecord.colonistsLaunched++;
				}
			}
			GameVictoryUtility.ShowCredits(GameVictoryUtility.MakeEndCredits("GameOverArchotechInvokedIntro".Translate(), "GameOverArchotechInvokedEnding".Translate(), stringBuilder.ToString(), "GameOverColonistsTranscended", list), SongDefOf.ArchonexusVictorySong, exitToMainMenu: true, 2.5f);
		}

		public static void ArchonexusCountdownUpdate()
		{
			if (timeLeft > 0f)
			{
				timeLeft -= Time.deltaTime;
				if (timeLeft <= 0f)
				{
					EndGame();
				}
			}
		}
	}
}
