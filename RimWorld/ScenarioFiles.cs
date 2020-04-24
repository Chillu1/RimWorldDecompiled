using System.Collections.Generic;
using System.IO;
using Verse;
using Verse.Steam;

namespace RimWorld
{
	public static class ScenarioFiles
	{
		private static List<Scenario> scenariosLocal = new List<Scenario>();

		private static List<Scenario> scenariosWorkshop = new List<Scenario>();

		public static IEnumerable<Scenario> AllScenariosLocal => scenariosLocal;

		public static IEnumerable<Scenario> AllScenariosWorkshop => scenariosWorkshop;

		public static void RecacheData()
		{
			scenariosLocal.Clear();
			foreach (FileInfo allCustomScenarioFile in GenFilePaths.AllCustomScenarioFiles)
			{
				if (GameDataSaveLoader.TryLoadScenario(allCustomScenarioFile.FullName, ScenarioCategory.CustomLocal, out Scenario scen))
				{
					scenariosLocal.Add(scen);
				}
			}
			scenariosWorkshop.Clear();
			foreach (WorkshopItem allSubscribedItem in WorkshopItems.AllSubscribedItems)
			{
				WorkshopItem_Scenario workshopItem_Scenario = allSubscribedItem as WorkshopItem_Scenario;
				if (workshopItem_Scenario != null)
				{
					scenariosWorkshop.Add(workshopItem_Scenario.GetScenario());
				}
			}
		}
	}
}
