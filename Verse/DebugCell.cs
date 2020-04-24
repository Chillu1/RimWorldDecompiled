using UnityEngine;

namespace Verse
{
	internal sealed class DebugCell
	{
		public IntVec3 c;

		public string displayString;

		public float colorPct;

		public int ticksLeft;

		public Material customMat;

		public void Draw()
		{
			if (customMat != null)
			{
				CellRenderer.RenderCell(c, customMat);
			}
			else
			{
				CellRenderer.RenderCell(c, colorPct);
			}
		}

		public void OnGUI()
		{
			if (displayString != null)
			{
				Vector2 vector = c.ToUIPosition();
				Rect rect = new Rect(vector.x - 20f, vector.y - 20f, 40f, 40f);
				if (new Rect(0f, 0f, UI.screenWidth, UI.screenHeight).Overlaps(rect))
				{
					Widgets.Label(rect, displayString);
				}
			}
		}
	}
}
