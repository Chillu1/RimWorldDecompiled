using RimWorld;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	public class Dialog_DebugSettingsMenu : Dialog_DebugOptionLister
	{
		public override bool IsDebug => true;

		public Dialog_DebugSettingsMenu()
		{
			forcePause = true;
		}

		protected override void DoListingItems()
		{
			if (KeyBindingDefOf.Dev_ToggleDebugSettingsMenu.KeyDownEvent)
			{
				Event.current.Use();
				Close();
			}
			Text.Font = GameFont.Small;
			listing.Label("Gameplay");
			FieldInfo[] fields = typeof(DebugSettings).GetFields();
			foreach (FieldInfo fi in fields)
			{
				DoField(fi);
			}
			listing.Gap(36f);
			Text.Font = GameFont.Small;
			listing.Label("View");
			fields = typeof(DebugViewSettings).GetFields();
			foreach (FieldInfo fi2 in fields)
			{
				DoField(fi2);
			}
		}

		private void DoField(FieldInfo fi)
		{
			if (fi.IsLiteral)
			{
				return;
			}
			string label = GenText.SplitCamelCase(fi.Name).CapitalizeFirst();
			bool checkOn = (bool)fi.GetValue(null);
			bool flag = checkOn;
			CheckboxLabeledDebug(label, ref checkOn);
			if (checkOn != flag)
			{
				fi.SetValue(null, checkOn);
				MethodInfo method = fi.DeclaringType.GetMethod(fi.Name + "Toggled", BindingFlags.Static | BindingFlags.Public);
				if (method != null)
				{
					method.Invoke(null, null);
				}
			}
		}
	}
}
