namespace LudeonTK;

public static class DebugTools
{
	public static DebugTool curTool;

	public static MeasureTool curMeasureTool;

	public static void DebugToolsOnGUI()
	{
		if (curTool != null)
		{
			curTool.DebugToolOnGUI();
		}
		if (curMeasureTool != null)
		{
			curMeasureTool.DebugToolOnGUI();
		}
	}
}
