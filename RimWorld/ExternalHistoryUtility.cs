using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class ExternalHistoryUtility
	{
		private static List<FileInfo> cachedFiles;

		private static int gameplayIDLength;

		private static string gameplayIDAvailableChars;

		public static IEnumerable<FileInfo> Files
		{
			get
			{
				int i = 0;
				while (i < cachedFiles.Count)
				{
					yield return cachedFiles[i];
					int num = i + 1;
					i = num;
				}
			}
		}

		static ExternalHistoryUtility()
		{
			gameplayIDLength = 20;
			gameplayIDAvailableChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
			try
			{
				cachedFiles = GenFilePaths.AllExternalHistoryFiles.ToList();
			}
			catch (Exception ex)
			{
				Log.Error("Could not get external history files: " + ex.Message);
			}
		}

		public static ExternalHistory Load(string path)
		{
			ExternalHistory externalHistory = null;
			try
			{
				externalHistory = new ExternalHistory();
				Scribe.loader.InitLoading(path);
				try
				{
					Scribe_Deep.Look(ref externalHistory, "externalHistory");
					Scribe.loader.FinalizeLoading();
					return externalHistory;
				}
				catch
				{
					Scribe.ForceStop();
					throw;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Could not load external history (" + path + "): " + ex.Message);
				return null;
			}
		}

		public static string GetRandomGameplayID()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < gameplayIDLength; i++)
			{
				int index = Rand.Range(0, gameplayIDAvailableChars.Length);
				stringBuilder.Append(gameplayIDAvailableChars[index]);
			}
			return stringBuilder.ToString();
		}

		public static bool IsValidGameplayID(string ID)
		{
			if (ID.NullOrEmpty() || ID.Length != gameplayIDLength)
			{
				return false;
			}
			for (int i = 0; i < ID.Length; i++)
			{
				bool flag = false;
				for (int j = 0; j < gameplayIDAvailableChars.Length; j++)
				{
					if (ID[i] == gameplayIDAvailableChars[j])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}

		public static string GetCurrentUploadDate()
		{
			return DateTime.UtcNow.ToString("yyMMdd");
		}

		public static int GetCurrentUploadTime()
		{
			return (int)(DateTime.UtcNow.TimeOfDay.TotalSeconds / 2.0);
		}
	}
}
