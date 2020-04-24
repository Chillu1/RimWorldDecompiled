using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class ScenarioLister
	{
		private static bool dirty = true;

		public static IEnumerable<Scenario> AllScenarios()
		{
			RecacheIfDirty();
			foreach (ScenarioDef allDef in DefDatabase<ScenarioDef>.AllDefs)
			{
				yield return allDef.scenario;
			}
			foreach (Scenario item in ScenarioFiles.AllScenariosLocal)
			{
				yield return item;
			}
			foreach (Scenario item2 in ScenarioFiles.AllScenariosWorkshop)
			{
				yield return item2;
			}
		}

		public static IEnumerable<Scenario> ScenariosInCategory(ScenarioCategory cat)
		{
			RecacheIfDirty();
			switch (cat)
			{
			case ScenarioCategory.FromDef:
				foreach (ScenarioDef allDef in DefDatabase<ScenarioDef>.AllDefs)
				{
					yield return allDef.scenario;
				}
				break;
			case ScenarioCategory.CustomLocal:
				foreach (Scenario item in ScenarioFiles.AllScenariosLocal)
				{
					yield return item;
				}
				break;
			case ScenarioCategory.SteamWorkshop:
				foreach (Scenario item2 in ScenarioFiles.AllScenariosWorkshop)
				{
					yield return item2;
				}
				break;
			}
		}

		public static bool ScenarioIsListedAnywhere(Scenario scen)
		{
			RecacheIfDirty();
			foreach (ScenarioDef allDef in DefDatabase<ScenarioDef>.AllDefs)
			{
				if (allDef.scenario == scen)
				{
					return true;
				}
			}
			foreach (Scenario item in ScenarioFiles.AllScenariosLocal)
			{
				if (scen == item)
				{
					return true;
				}
			}
			return false;
		}

		public static void MarkDirty()
		{
			dirty = true;
		}

		private static void RecacheIfDirty()
		{
			if (dirty)
			{
				RecacheData();
			}
		}

		private static void RecacheData()
		{
			dirty = false;
			int num = ScenarioListHash();
			ScenarioFiles.RecacheData();
			if (ScenarioListHash() != num && !LongEventHandler.ShouldWaitForEvent)
			{
				Find.WindowStack.WindowOfType<Page_SelectScenario>()?.Notify_ScenarioListChanged();
			}
		}

		public static int ScenarioListHash()
		{
			int num = 9826121;
			foreach (Scenario item in AllScenarios())
			{
				num ^= 791 * item.GetHashCode() * 6121;
			}
			return num;
		}
	}
}
