using System;
using System.IO;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Dialog_SaveFileList : Dialog_FileList
	{
		private static readonly Color AutosaveTextColor = new Color(0.75f, 0.75f, 0.75f);

		protected override Color FileNameColor(SaveFileInfo sfi)
		{
			if (SaveGameFilesUtility.IsAutoSave(Path.GetFileNameWithoutExtension(sfi.FileInfo.Name)))
			{
				GUI.color = AutosaveTextColor;
			}
			return base.FileNameColor(sfi);
		}

		protected override void ReloadFiles()
		{
			files.Clear();
			foreach (FileInfo allSavedGameFile in GenFilePaths.AllSavedGameFiles)
			{
				try
				{
					files.Add(new SaveFileInfo(allSavedGameFile));
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + allSavedGameFile.Name + ": " + ex.ToString());
				}
			}
		}

		public override void PostClose()
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Menu);
			}
		}
	}
}
