using System;
using System.IO;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_IdeoList : Dialog_FileList
	{
		protected override void ReloadFiles()
		{
			files.Clear();
			foreach (FileInfo allCustomIdeoFile in GenFilePaths.AllCustomIdeoFiles)
			{
				try
				{
					SaveFileInfo saveFileInfo = new SaveFileInfo(allCustomIdeoFile);
					saveFileInfo.LoadData();
					files.Add(saveFileInfo);
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + allCustomIdeoFile.Name + ": " + ex.ToString());
				}
			}
		}
	}
}
