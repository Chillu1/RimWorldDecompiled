namespace Verse
{
	public static class DebugTools
	{
		public static DebugTool curTool;

		public static void DebugToolsOnGUI()
		{
			if (curTool != null)
			{
				curTool.DebugToolOnGUI();
			}
		}
	}
}
