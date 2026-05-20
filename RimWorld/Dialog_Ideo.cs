using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_Ideo : Window
	{
		private Ideo ideo;

		private float viewHeight;

		private Vector2 scrollPosition;

		public override Vector2 InitialSize => new Vector2(600f, Mathf.Min(1000f, UI.screenHeight));

		public Dialog_Ideo(Ideo ideo)
		{
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
			this.ideo = ideo;
		}

		public override void PostClose()
		{
			base.PostClose();
			IdeoUIUtility.UnselectCurrent();
		}

		public override void DoWindowContents(Rect inRect)
		{
			inRect.height -= Window.CloseButSize.y;
			IdeoUIUtility.DoIdeoDetails(inRect, ideo, ref scrollPosition, ref viewHeight);
		}
	}
}
