using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace RimWorld
{
	public static class BackstoryDatabase
	{
		public static Dictionary<string, Backstory> allBackstories = new Dictionary<string, Backstory>();

		private static Dictionary<Pair<BackstorySlot, BackstoryCategoryFilter>, List<Backstory>> shuffleableBackstoryList = new Dictionary<Pair<BackstorySlot, BackstoryCategoryFilter>, List<Backstory>>();

		private static Regex regex = new Regex("^[^0-9]*");

		public static void Clear()
		{
			allBackstories.Clear();
		}

		public static void ReloadAllBackstories()
		{
			foreach (Backstory item in DirectXmlLoader.LoadXmlDataInResourcesFolder<Backstory>("Backstories/Shuffled"))
			{
				DeepProfiler.Start("Backstory.PostLoad");
				try
				{
					item.PostLoad();
				}
				finally
				{
					DeepProfiler.End();
				}
				DeepProfiler.Start("Backstory.ResolveReferences");
				try
				{
					item.ResolveReferences();
				}
				finally
				{
					DeepProfiler.End();
				}
				foreach (string item2 in item.ConfigErrors(ignoreNoSpawnCategories: false))
				{
					Log.Error(item.title + ": " + item2);
				}
				DeepProfiler.Start("AddBackstory");
				try
				{
					AddBackstory(item);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			SolidBioDatabase.LoadAllBios();
		}

		public static void AddBackstory(Backstory bs)
		{
			if (allBackstories.ContainsKey(bs.identifier))
			{
				if (bs == allBackstories[bs.identifier])
				{
					Log.Error("Tried to add the same backstory twice " + bs.identifier);
					return;
				}
				Log.Error("Backstory " + bs.title + " has same unique save key " + bs.identifier + " as old backstory " + allBackstories[bs.identifier].title);
			}
			else
			{
				allBackstories.Add(bs.identifier, bs);
				shuffleableBackstoryList.Clear();
			}
		}

		public static bool TryGetWithIdentifier(string identifier, out Backstory bs, bool closestMatchWarning = true)
		{
			identifier = GetIdentifierClosestMatch(identifier, closestMatchWarning);
			return allBackstories.TryGetValue(identifier, out bs);
		}

		public static string GetIdentifierClosestMatch(string identifier, bool closestMatchWarning = true)
		{
			if (allBackstories.ContainsKey(identifier))
			{
				return identifier;
			}
			string b = StripNumericSuffix(identifier);
			foreach (KeyValuePair<string, Backstory> allBackstory in allBackstories)
			{
				Backstory value = allBackstory.Value;
				if (StripNumericSuffix(value.identifier) == b)
				{
					if (closestMatchWarning)
					{
						Log.Warning("Couldn't find exact match for backstory " + identifier + ", using closest match " + value.identifier);
					}
					return value.identifier;
				}
			}
			Log.Warning("Couldn't find exact match for backstory " + identifier + ", or any close match.");
			return identifier;
		}

		public static Backstory RandomBackstory(BackstorySlot slot)
		{
			return allBackstories.Where((KeyValuePair<string, Backstory> bs) => bs.Value.slot == slot).RandomElement().Value;
		}

		public static List<Backstory> ShuffleableBackstoryList(BackstorySlot slot, BackstoryCategoryFilter group)
		{
			Pair<BackstorySlot, BackstoryCategoryFilter> key = new Pair<BackstorySlot, BackstoryCategoryFilter>(slot, group);
			if (!shuffleableBackstoryList.ContainsKey(key))
			{
				shuffleableBackstoryList[key] = allBackstories.Values.Where((Backstory bs) => bs.shuffleable && bs.slot == slot && group.Matches(bs)).ToList();
			}
			return shuffleableBackstoryList[key];
		}

		public static string StripNumericSuffix(string key)
		{
			return regex.Match(key).Captures[0].Value;
		}
	}
}
