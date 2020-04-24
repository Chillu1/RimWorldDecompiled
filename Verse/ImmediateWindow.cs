using System;
using UnityEngine;

namespace Verse
{
	public class ImmediateWindow : Window
	{
		public Action doWindowFunc;

		public override Vector2 InitialSize => windowRect.size;

		protected override float Margin => 0f;

		public ImmediateWindow()
		{
			doCloseButton = false;
			doCloseX = false;
			soundAppear = null;
			soundClose = null;
			closeOnClickedOutside = false;
			closeOnAccept = false;
			closeOnCancel = false;
			focusWhenOpened = false;
			preventCameraMotion = false;
		}

		public override void DoWindowContents(Rect inRect)
		{
			doWindowFunc();
		}
	}
}
