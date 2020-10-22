using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	public class Dialog_DebugOutputMenu : Dialog_DebugOptionLister
	{
		private struct DebugOutputOption
		{
			public string label;

			public string category;

			public Action action;
		}

		private List<DebugOutputOption> debugOutputs = new List<DebugOutputOption>();

		public const string DefaultCategory = "General";

		public override bool IsDebug => true;

		protected override int HighlightedIndex
		{
			get
			{
				if (FilterAllows(debugOutputs[prioritizedHighlightedIndex].label))
				{
					return prioritizedHighlightedIndex;
				}
				if (filter.NullOrEmpty())
				{
					return 0;
				}
				for (int i = 0; i < debugOutputs.Count; i++)
				{
					if (FilterAllows(debugOutputs[i].label))
					{
						currentHighlightIndex = i;
						break;
					}
				}
				return currentHighlightIndex;
			}
		}

		public Dialog_DebugOutputMenu()
		{
			forcePause = true;
			foreach (Type allType in GenTypes.AllTypes)
			{
				MethodInfo[] methods = allType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.TryGetAttribute<DebugOutputAttribute>(out var customAttribute))
					{
						GenerateCacheForMethod(methodInfo, customAttribute);
					}
				}
			}
			debugOutputs = (from r in debugOutputs
				orderby r.category, r.label
				select r).ToList();
		}

		private void GenerateCacheForMethod(MethodInfo method, DebugOutputAttribute attribute)
		{
			if (!attribute.onlyWhenPlaying || Current.ProgramState == ProgramState.Playing)
			{
				string label = attribute.name ?? GenText.SplitCamelCase(method.Name);
				Action action = Delegate.CreateDelegate(typeof(Action), method) as Action;
				string text = attribute.category;
				if (text == null)
				{
					text = "General";
				}
				debugOutputs.Add(new DebugOutputOption
				{
					label = label,
					category = text,
					action = action
				});
			}
		}

		protected override void DoListingItems()
		{
			base.DoListingItems();
			int highlightedIndex = HighlightedIndex;
			string b = null;
			for (int i = 0; i < debugOutputs.Count; i++)
			{
				DebugOutputOption debugOutputOption = debugOutputs[i];
				if (debugOutputOption.category != b)
				{
					DoLabel(debugOutputOption.category);
					b = debugOutputOption.category;
				}
				Log.openOnMessage = true;
				try
				{
					DebugAction_NewTmp(debugOutputOption.label, debugOutputOption.action, highlightedIndex == i);
				}
				finally
				{
					Log.openOnMessage = false;
				}
			}
		}

		protected override void ChangeHighlightedOption()
		{
			int highlightedIndex = HighlightedIndex;
			for (int i = 0; i < debugOutputs.Count; i++)
			{
				int num = (highlightedIndex + i + 1) % debugOutputs.Count;
				if (FilterAllows(debugOutputs[num].label))
				{
					prioritizedHighlightedIndex = num;
					break;
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
					Close();
					debugOutputs[highlightedIndex].action();
				}
				Event.current.Use();
			}
		}
	}
}
