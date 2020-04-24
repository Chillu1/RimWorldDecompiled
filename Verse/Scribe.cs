namespace Verse
{
	public static class Scribe
	{
		public static ScribeSaver saver = new ScribeSaver();

		public static ScribeLoader loader = new ScribeLoader();

		public static LoadSaveMode mode = LoadSaveMode.Inactive;

		public static void ForceStop()
		{
			mode = LoadSaveMode.Inactive;
			saver.ForceStop();
			loader.ForceStop();
		}

		public static bool EnterNode(string nodeName)
		{
			if (mode == LoadSaveMode.Inactive)
			{
				return false;
			}
			if (mode == LoadSaveMode.Saving)
			{
				return saver.EnterNode(nodeName);
			}
			if (mode == LoadSaveMode.LoadingVars || mode == LoadSaveMode.ResolvingCrossRefs || mode == LoadSaveMode.PostLoadInit)
			{
				return loader.EnterNode(nodeName);
			}
			return true;
		}

		public static void ExitNode()
		{
			if (mode == LoadSaveMode.Saving)
			{
				saver.ExitNode();
			}
			if (mode == LoadSaveMode.LoadingVars || mode == LoadSaveMode.ResolvingCrossRefs || mode == LoadSaveMode.PostLoadInit)
			{
				loader.ExitNode();
			}
		}
	}
}
