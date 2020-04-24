using UnityEngine;

namespace Verse
{
	internal class EditWindow_DefEditor : EditWindow
	{
		public Def def;

		private float viewHeight;

		private Vector2 scrollPosition;

		private float labelColumnWidth = 140f;

		private const float TopAreaHeight = 16f;

		private const float ExtraScrollHeight = 200f;

		public override Vector2 InitialSize => new Vector2(400f, 600f);

		public override bool IsDebug => true;

		public EditWindow_DefEditor(Def def)
		{
			this.def = def;
			closeOnAccept = false;
			closeOnCancel = false;
			optionalTitle = def.ToString();
		}

		public override void DoWindowContents(Rect inRect)
		{
			if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Escape))
			{
				UI.UnfocusCurrentControl();
			}
			Rect rect = new Rect(0f, 0f, inRect.width, 16f);
			labelColumnWidth = Widgets.HorizontalSlider(rect, labelColumnWidth, 0f, inRect.width);
			Rect outRect = inRect.AtZero();
			outRect.yMin += 16f;
			Rect rect2 = new Rect(0f, 0f, outRect.width - 16f, viewHeight);
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
			Listing_TreeDefs listing_TreeDefs = new Listing_TreeDefs(labelColumnWidth);
			listing_TreeDefs.Begin(rect2);
			TreeNode_Editor node = EditTreeNodeDatabase.RootOf(def);
			listing_TreeDefs.ContentLines(node, 0);
			listing_TreeDefs.End();
			if (Event.current.type == EventType.Layout)
			{
				viewHeight = listing_TreeDefs.CurHeight + 200f;
			}
			Widgets.EndScrollView();
		}
	}
}
