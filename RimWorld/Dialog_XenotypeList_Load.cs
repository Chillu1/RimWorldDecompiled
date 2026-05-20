using System;
using Verse;

namespace RimWorld
{
	public class Dialog_XenotypeList_Load : Dialog_XenotypeList
	{
		private Action<CustomXenotype> xenotypeReturner;

		public Dialog_XenotypeList_Load(Action<CustomXenotype> xenotypeReturner)
		{
			this.xenotypeReturner = xenotypeReturner;
			interactButLabel = "LoadGameButton".Translate();
			deleteTipKey = "DeleteThisXenotype";
		}

		protected override void DoFileInteraction(string fileName)
		{
			string filePath = GenFilePaths.AbsFilePathForXenotype(fileName);
			PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype, delegate
			{
				if (GameDataSaveLoader.TryLoadXenotype(filePath, out var xenotype))
				{
					xenotypeReturner(xenotype);
				}
				Close();
			});
		}
	}
}
