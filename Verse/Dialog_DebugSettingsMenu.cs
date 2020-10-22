using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Dialog_DebugSettingsMenu : Dialog_DebugOptionLister
	{
		private List<FieldInfo> settingsFields = new List<FieldInfo>();

		public override bool IsDebug => true;

		protected override int HighlightedIndex
		{
			get
			{
				if (FilterAllows(LegibleFieldName(settingsFields[prioritizedHighlightedIndex])))
				{
					return prioritizedHighlightedIndex;
				}
				if (filter.NullOrEmpty())
				{
					return 0;
				}
				for (int i = 0; i < settingsFields.Count; i++)
				{
					if (FilterAllows(LegibleFieldName(settingsFields[i])))
					{
						currentHighlightIndex = i;
						break;
					}
				}
				return currentHighlightIndex;
			}
		}

		public Dialog_DebugSettingsMenu()
		{
			forcePause = true;
			FieldInfo[] fields = typeof(DebugSettings).GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				if (!fieldInfo.IsLiteral)
				{
					settingsFields.Add(fieldInfo);
				}
			}
			fields = typeof(DebugViewSettings).GetFields();
			foreach (FieldInfo fieldInfo2 in fields)
			{
				if (!fieldInfo2.IsLiteral)
				{
					settingsFields.Add(fieldInfo2);
				}
			}
		}

		protected override void DoListingItems()
		{
			base.DoListingItems();
			if (KeyBindingDefOf.Dev_ToggleDebugSettingsMenu.KeyDownEvent)
			{
				Event.current.Use();
				Close();
			}
			Text.Font = GameFont.Small;
			listing.Label("Gameplay");
			int highlightedIndex = HighlightedIndex;
			int num = 0;
			FieldInfo[] fields = typeof(DebugSettings).GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				if (!fieldInfo.IsLiteral)
				{
					DoField_NewTmp(fieldInfo, highlightedIndex == num);
					num++;
				}
			}
			listing.Gap(36f);
			Text.Font = GameFont.Small;
			listing.Label("View");
			fields = typeof(DebugViewSettings).GetFields();
			foreach (FieldInfo fieldInfo2 in fields)
			{
				if (!fieldInfo2.IsLiteral)
				{
					DoField_NewTmp(fieldInfo2, highlightedIndex == num);
					num++;
				}
			}
		}

		public override void OnAcceptKeyPressed()
		{
			if (GUI.GetNameOfFocusedControl() == "DebugFilter")
			{
				int highlightedIndex = HighlightedIndex;
				if (highlightedIndex >= 0)
				{
					Toggle(highlightedIndex);
				}
				Event.current.Use();
			}
		}

		protected override void ChangeHighlightedOption()
		{
			int highlightedIndex = HighlightedIndex;
			for (int i = 0; i < settingsFields.Count; i++)
			{
				int num = (highlightedIndex + i + 1) % settingsFields.Count;
				if (FilterAllows(LegibleFieldName(settingsFields[num])))
				{
					prioritizedHighlightedIndex = num;
					break;
				}
			}
		}

		[Obsolete("Only used for mod compatibility.")]
		private void DoField(FieldInfo fi)
		{
			DoField_NewTmp(fi, highlight: false);
		}

		private void DoField_NewTmp(FieldInfo fi, bool highlight)
		{
			if (fi.IsLiteral)
			{
				return;
			}
			string label = GenText.SplitCamelCase(fi.Name).CapitalizeFirst();
			bool checkOn = (bool)fi.GetValue(null);
			bool flag = checkOn;
			CheckboxLabeledDebug_NewTmp(label, ref checkOn, highlight);
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

		private void Toggle(int index)
		{
			FieldInfo fieldInfo = settingsFields[index];
			bool flag = (bool)fieldInfo.GetValue(null);
			fieldInfo.SetValue(null, !flag);
			MethodInfo method = fieldInfo.DeclaringType.GetMethod(fieldInfo.Name + "Toggled", BindingFlags.Static | BindingFlags.Public);
			if (method != null)
			{
				method.Invoke(null, null);
			}
		}

		private string LegibleFieldName(FieldInfo fi)
		{
			return GenText.SplitCamelCase(fi.Name).CapitalizeFirst();
		}
	}
}
