using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Dialog_ResolutionConfirm : Window
	{
		private float startTime;

		private IntVec2 oldRes;

		private bool oldFullscreen;

		private float oldUIScale;

		private const float RevertTime = 10f;

		private float TimeUntilRevert => startTime + 10f - Time.realtimeSinceStartup;

		public override Vector2 InitialSize => new Vector2(500f, 300f);

		private Dialog_ResolutionConfirm()
		{
			startTime = Time.realtimeSinceStartup;
			closeOnAccept = false;
			closeOnCancel = false;
			absorbInputAroundWindow = true;
		}

		public Dialog_ResolutionConfirm(bool oldFullscreen)
			: this()
		{
			this.oldFullscreen = oldFullscreen;
			oldRes = new IntVec2(Screen.width, Screen.height);
			oldUIScale = Prefs.UIScale;
		}

		public Dialog_ResolutionConfirm(IntVec2 oldRes)
			: this()
		{
			oldFullscreen = Screen.fullScreen;
			this.oldRes = oldRes;
			oldUIScale = Prefs.UIScale;
		}

		public Dialog_ResolutionConfirm(float oldUIScale)
			: this()
		{
			oldFullscreen = Screen.fullScreen;
			oldRes = new IntVec2(Screen.width, Screen.height);
			this.oldUIScale = oldUIScale;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			string label = "ConfirmResolutionChange".Translate(Mathf.CeilToInt(TimeUntilRevert));
			Widgets.Label(new Rect(0f, 0f, inRect.width, inRect.height), label);
			if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "ResolutionKeep".Translate()))
			{
				Close();
			}
			if (Widgets.ButtonText(new Rect(inRect.width / 2f + 20f, inRect.height - 35f, inRect.width / 2f - 20f, 35f), "ResolutionRevert".Translate()))
			{
				Revert();
				Close();
			}
		}

		private void Revert()
		{
			if (Prefs.LogVerbose)
			{
				Log.Message("Reverting screen settings to " + oldRes.x + "x" + oldRes.z + ", fs=" + oldFullscreen.ToString());
			}
			ResolutionUtility.SetResolutionRaw(oldRes.x, oldRes.z, oldFullscreen);
			Prefs.FullScreen = oldFullscreen;
			Prefs.ScreenWidth = oldRes.x;
			Prefs.ScreenHeight = oldRes.z;
			Prefs.UIScale = oldUIScale;
			GenUI.ClearLabelWidthCache();
		}

		public override void WindowUpdate()
		{
			if (TimeUntilRevert <= 0f)
			{
				Revert();
				Close();
			}
		}
	}
}
