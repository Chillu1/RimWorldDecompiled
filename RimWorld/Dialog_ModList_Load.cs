using System;
using Verse;

namespace RimWorld
{
	public class Dialog_ModList_Load : Dialog_ModList
	{
		private Action<ModList> modListReturner;

		public Dialog_ModList_Load(Action<ModList> modListReturner)
		{
			interactButLabel = "Load".Translate();
			this.modListReturner = modListReturner;
		}

		protected override void DoFileInteraction(string fileName)
		{
			string text = GenFilePaths.AbsFilePathForModList(fileName);
			try
			{
				Scribe.loader.InitLoadingMetaHeaderOnly(text);
				ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.ModList, logVersionConflictWarning: false);
				Scribe.loader.FinalizeLoading();
				ModList modList = null;
				if (GameDataSaveLoader.TryLoadModList(text, out modList))
				{
					modListReturner(modList);
				}
				Close();
			}
			catch (Exception ex)
			{
				Log.Warning("Exception loading " + text + ": " + ex);
				Scribe.ForceStop();
			}
		}
	}
}
