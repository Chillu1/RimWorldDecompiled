using System;
using System.IO;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_XenotypeList : Dialog_FileList
	{
		protected override void ReloadFiles()
		{
			files.Clear();
			foreach (FileInfo allCustomXenotypeFile in GenFilePaths.AllCustomXenotypeFiles)
			{
				try
				{
					SaveFileInfo saveFileInfo = new SaveFileInfo(allCustomXenotypeFile);
					saveFileInfo.LoadData();
					files.Add(saveFileInfo);
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + allCustomXenotypeFile.Name + ": " + ex.ToString());
				}
			}
			CharacterCardUtility.cachedCustomXenotypes = null;
		}
	}
}
