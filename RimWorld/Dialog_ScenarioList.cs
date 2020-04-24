using System;
using System.IO;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_ScenarioList : Dialog_FileList
	{
		protected override void ReloadFiles()
		{
			files.Clear();
			foreach (FileInfo allCustomScenarioFile in GenFilePaths.AllCustomScenarioFiles)
			{
				try
				{
					files.Add(new SaveFileInfo(allCustomScenarioFile));
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + allCustomScenarioFile.Name + ": " + ex.ToString());
				}
			}
		}
	}
}
