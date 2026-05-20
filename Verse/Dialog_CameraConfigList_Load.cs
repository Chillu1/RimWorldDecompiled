using System;

namespace Verse
{
	public class Dialog_CameraConfigList_Load : Dialog_CameraConfigList
	{
		private Action<CameraMapConfig> configReturner;

		public Dialog_CameraConfigList_Load(Action<CameraMapConfig> cfgReturner)
		{
			configReturner = cfgReturner;
			interactButLabel = "Load config";
		}

		protected override void DoFileInteraction(string fileName)
		{
			string filePath = GenFilePaths.AbsFilePathForCameraConfig(fileName);
			PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.CameraConfig, delegate
			{
				if (GameDataSaveLoader.TryLoadCameraConfig(filePath, out var config))
				{
					configReturner(config);
				}
				Close();
			});
		}
	}
}
