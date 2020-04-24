using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldGlobalControls
	{
		public const float Width = 200f;

		private const int VisibilityControlsPerRow = 5;

		private WidgetRow rowVisibility = new WidgetRow();

		public void WorldGlobalControlsOnGUI()
		{
			if (Event.current.type == EventType.Layout)
			{
				return;
			}
			float leftX = (float)UI.screenWidth - 200f;
			float curBaseY = (float)UI.screenHeight - 4f;
			if (Current.ProgramState == ProgramState.Playing)
			{
				curBaseY -= 35f;
			}
			GlobalControlsUtility.DoPlaySettings(rowVisibility, worldView: true, ref curBaseY);
			if (Current.ProgramState == ProgramState.Playing)
			{
				curBaseY -= 4f;
				GlobalControlsUtility.DoTimespeedControls(leftX, 200f, ref curBaseY);
				if (Find.CurrentMap != null || Find.WorldSelector.AnyObjectOrTileSelected)
				{
					curBaseY -= 4f;
					GlobalControlsUtility.DoDate(leftX, 200f, ref curBaseY);
				}
				float num = 154f;
				float num2 = Find.World.gameConditionManager.TotalHeightAt(num);
				Rect rect = new Rect((float)UI.screenWidth - num, curBaseY - num2, num, num2);
				Find.World.gameConditionManager.DoConditionsUI(rect);
				curBaseY -= rect.height;
			}
			if (Prefs.ShowRealtimeClock)
			{
				GlobalControlsUtility.DoRealtimeClock(leftX, 200f, ref curBaseY);
			}
			Find.WorldRoutePlanner.DoRoutePlannerButton(ref curBaseY);
			if (!Find.PlaySettings.lockNorthUp)
			{
				CompassWidget.CompassOnGUI(ref curBaseY);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				curBaseY -= 10f;
				Find.LetterStack.LettersOnGUI(curBaseY);
			}
		}
	}
}
