using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GameVictoryUtility
	{
		public static string MakeEndCredits(string intro, string ending, string escapees)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(intro);
			if (!escapees.NullOrEmpty())
			{
				stringBuilder.Append(" ");
				stringBuilder.Append("GameOverColonistsEscaped".Translate(escapees));
			}
			stringBuilder.AppendLine();
			string text = PawnsLeftBehind();
			if (!text.NullOrEmpty())
			{
				stringBuilder.AppendLine("GameOverColonistsLeft".Translate(text));
			}
			stringBuilder.AppendLine(ending);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(InMemoryOfSection());
			return stringBuilder.ToString();
		}

		public static void ShowCredits(string victoryText)
		{
			Screen_Credits screen_Credits = new Screen_Credits(victoryText);
			screen_Credits.wonGame = true;
			Find.WindowStack.Add(screen_Credits);
			Find.MusicManagerPlay.ForceSilenceFor(999f);
			ScreenFader.StartFade(Color.clear, 3f);
		}

		public static string PawnsLeftBehind()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				stringBuilder.AppendLine("   " + item.LabelCap);
			}
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				Caravan caravan = caravans[i];
				if (!caravan.IsPlayerControlled)
				{
					continue;
				}
				List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
				for (int j = 0; j < pawnsListForReading.Count; j++)
				{
					Pawn pawn = pawnsListForReading[j];
					if (pawn.IsColonist && pawn.HostFaction == null)
					{
						stringBuilder.AppendLine("   " + pawn.LabelCap);
					}
				}
			}
			if (stringBuilder.Length == 0)
			{
				return string.Empty;
			}
			return stringBuilder.ToString();
		}

		public static string InMemoryOfSection()
		{
			StringBuilder stringBuilder = new StringBuilder();
			IEnumerable<Pawn> enumerable = Find.WorldPawns.AllPawnsDead.Where((Pawn p) => p.IsColonist);
			if (enumerable.Any())
			{
				stringBuilder.AppendLine("GameOverInMemoryOf".Translate());
				foreach (Pawn item in enumerable)
				{
					stringBuilder.AppendLine("   " + item.LabelCap);
				}
			}
			return stringBuilder.ToString();
		}
	}
}
