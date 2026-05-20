using System;
using System.IO;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_ModList : Dialog_FileList
	{
		protected override void ReloadFiles()
		{
			files.Clear();
			foreach (FileInfo allModListFile in GenFilePaths.AllModListFiles)
			{
				try
				{
					SaveFileInfo saveFileInfo = new SaveFileInfo(allModListFile);
					saveFileInfo.LoadData();
					files.Add(saveFileInfo);
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + allModListFile.Name + ": " + ex.ToString());
				}
			}
		}
	}
}
