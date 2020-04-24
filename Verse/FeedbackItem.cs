using UnityEngine;

namespace Verse
{
	public abstract class FeedbackItem
	{
		protected Vector2 FloatPerSecond = new Vector2(20f, -20f);

		private int uniqueID;

		public float TimeLeft = 2f;

		protected Vector2 CurScreenPos;

		private static int freeUniqueID;

		public FeedbackItem(Vector2 ScreenPos)
		{
			uniqueID = freeUniqueID++;
			CurScreenPos = ScreenPos;
			CurScreenPos.y -= 15f;
		}

		public void Update()
		{
			TimeLeft -= Time.deltaTime;
			CurScreenPos += FloatPerSecond * Time.deltaTime;
		}

		public abstract void FeedbackOnGUI();

		protected void DrawFloatingText(string str, Color TextColor)
		{
			float x = Text.CalcSize(str).x;
			Rect wordRect = new Rect(CurScreenPos.x - x / 2f, CurScreenPos.y, x, 20f);
			Find.WindowStack.ImmediateWindow(5983 * uniqueID + 495, wordRect, WindowLayer.Super, delegate
			{
				Rect rect = wordRect.AtZero();
				Text.Anchor = TextAnchor.UpperCenter;
				Text.Font = GameFont.Small;
				GUI.DrawTexture(rect, TexUI.GrayTextBG);
				GUI.color = TextColor;
				Widgets.Label(rect, str);
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}, doBackground: false);
		}
	}
}
