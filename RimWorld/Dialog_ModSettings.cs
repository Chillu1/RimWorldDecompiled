using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ModSettings : Window
	{
		private Mod mod;

		private const float TopAreaHeight = 40f;

		private const float TopButtonHeight = 35f;

		private const float TopButtonWidth = 150f;

		public override Vector2 InitialSize => new Vector2(900f, 700f);

		public Dialog_ModSettings(Mod mod)
		{
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
			this.mod = mod;
		}

		public override void PreClose()
		{
			base.PreClose();
			mod.WriteSettings();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), mod.SettingsCategory());
			Text.Font = GameFont.Small;
			Rect inRect2 = new Rect(0f, 40f, inRect.width, inRect.height - 40f - Window.CloseButSize.y);
			mod.DoSettingsWindowContents(inRect2);
		}
	}
}
