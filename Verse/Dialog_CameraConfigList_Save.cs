using RimWorld;

namespace Verse
{
	public class Dialog_CameraConfigList_Save : Dialog_CameraConfigList
	{
		private CameraMapConfig config;

		protected override bool ShouldDoTypeInField => true;

		public Dialog_CameraConfigList_Save(CameraMapConfig config)
		{
			interactButLabel = "OverwriteButton".Translate();
			this.config = config;
		}

		protected override void DoFileInteraction(string fileName)
		{
			fileName = GenFile.SanitizedFileName(fileName);
			string absPath = GenFilePaths.AbsFilePathForCameraConfig(fileName);
			LongEventHandler.QueueLongEvent(delegate
			{
				GameDataSaveLoader.SaveCameraConfig(config, absPath);
			}, "SavingLongEvent", doAsynchronously: false, null);
			Messages.Message("SavedAs".Translate(fileName), MessageTypeDefOf.SilentInput, historical: false);
			Close();
		}
	}
}
