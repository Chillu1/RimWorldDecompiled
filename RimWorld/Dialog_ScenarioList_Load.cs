using System;
using Verse;

namespace RimWorld
{
	public class Dialog_ScenarioList_Load : Dialog_ScenarioList
	{
		private Action<Scenario> scenarioReturner;

		public Dialog_ScenarioList_Load(Action<Scenario> scenarioReturner)
		{
			interactButLabel = "LoadGameButton".Translate();
			this.scenarioReturner = scenarioReturner;
		}

		protected override void DoFileInteraction(string fileName)
		{
			string filePath = GenFilePaths.AbsPathForScenario(fileName);
			PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Scenario, delegate
			{
				Scenario scen = null;
				if (GameDataSaveLoader.TryLoadScenario(filePath, ScenarioCategory.CustomLocal, out scen))
				{
					scenarioReturner(scen);
				}
				Close();
			});
		}
	}
}
