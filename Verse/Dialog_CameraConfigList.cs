using System;
using System.IO;
using RimWorld;

namespace Verse
{
	public abstract class Dialog_CameraConfigList : Dialog_FileList
	{
		protected override void ReloadFiles()
		{
			files.Clear();
			foreach (FileInfo allCameraConfigFile in GenFilePaths.AllCameraConfigFiles)
			{
				try
				{
					SaveFileInfo saveFileInfo = new SaveFileInfo(allCameraConfigFile);
					saveFileInfo.LoadData();
					files.Add(saveFileInfo);
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + allCameraConfigFile.Name + ": " + ex.ToString());
				}
			}
		}
	}
}
