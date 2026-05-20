using System;
using System.IO;
using Verse;

namespace RimWorld;

public abstract class Dialog_ScenarioList : Dialog_FileList
{
	protected override void ReloadFiles()
	{
		files.Clear();
		foreach (FileInfo allCustomScenarioFile in GenFilePaths.AllCustomScenarioFiles)
		{
			try
			{
				SaveFileInfo saveFileInfo = new SaveFileInfo(allCustomScenarioFile);
				saveFileInfo.LoadData();
				files.Add(saveFileInfo);
			}
			catch (Exception arg)
			{
				Log.Error($"Exception loading {allCustomScenarioFile.Name}: {arg}");
			}
		}
	}
}
