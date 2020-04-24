namespace Verse
{
	internal static class DraggableResultUtility
	{
		public static bool AnyPressed(this Widgets.DraggableResult result)
		{
			if (result != Widgets.DraggableResult.Pressed)
			{
				return result == Widgets.DraggableResult.DraggedThenPressed;
			}
			return true;
		}
	}
}
