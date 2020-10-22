using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	public class Dialog_DebugActionsMenu : Dialog_DebugOptionLister
	{
		public struct DebugActionOption
		{
			public DebugActionType actionType;

			public string label;

			public string category;

			public Action action;

			public Action<Pawn> pawnAction;
		}

		private List<DebugActionOption> debugActions = new List<DebugActionOption>();

		public override bool IsDebug => true;

		protected override int HighlightedIndex
		{
			get
			{
				if (FilterAllows(debugActions[prioritizedHighlightedIndex].label))
				{
					return prioritizedHighlightedIndex;
				}
				if (filter.NullOrEmpty())
				{
					return 0;
				}
				for (int i = 0; i < debugActions.Count; i++)
				{
					if (FilterAllows(debugActions[i].label))
					{
						currentHighlightIndex = i;
						break;
					}
				}
				return currentHighlightIndex;
			}
		}

		public Dialog_DebugActionsMenu()
		{
			forcePause = true;
			foreach (Type allType in GenTypes.AllTypes)
			{
				MethodInfo[] methods = allType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.TryGetAttribute<DebugActionAttribute>(out var customAttribute))
					{
						GenerateCacheForMethod(methodInfo, customAttribute);
					}
					if (!methodInfo.TryGetAttribute<DebugActionYielderAttribute>(out var _))
					{
						continue;
					}
					foreach (DebugActionOption item in (IEnumerable<DebugActionOption>)methodInfo.Invoke(null, null))
					{
						debugActions.Add(item);
					}
				}
			}
			debugActions = (from r in debugActions
				orderby DebugActionCategories.GetOrderFor(r.category), r.category
				select r).ToList();
		}

		private void GenerateCacheForMethod(MethodInfo method, DebugActionAttribute attribute)
		{
			if (attribute.IsAllowedInCurrentGameState)
			{
				string text = (string.IsNullOrEmpty(attribute.name) ? GenText.SplitCamelCase(method.Name) : attribute.name);
				if (attribute.actionType == DebugActionType.ToolMap || attribute.actionType == DebugActionType.ToolMapForPawns || attribute.actionType == DebugActionType.ToolWorld)
				{
					text = "T: " + text;
				}
				string category = attribute.category;
				DebugActionOption debugActionOption = default(DebugActionOption);
				debugActionOption.label = text;
				debugActionOption.category = category;
				debugActionOption.actionType = attribute.actionType;
				DebugActionOption item = debugActionOption;
				if (attribute.actionType == DebugActionType.ToolMapForPawns)
				{
					item.pawnAction = Delegate.CreateDelegate(typeof(Action<Pawn>), method) as Action<Pawn>;
				}
				else
				{
					item.action = Delegate.CreateDelegate(typeof(Action), method) as Action;
				}
				debugActions.Add(item);
			}
		}

		protected override void DoListingItems()
		{
			base.DoListingItems();
			int highlightedIndex = HighlightedIndex;
			string b = null;
			for (int i = 0; i < debugActions.Count; i++)
			{
				DebugActionOption debugActionOption = debugActions[i];
				bool highlight = highlightedIndex == i;
				if (debugActionOption.category != b)
				{
					DoGap();
					DoLabel(debugActionOption.category);
					b = debugActionOption.category;
				}
				Log.openOnMessage = true;
				try
				{
					switch (debugActionOption.actionType)
					{
					case DebugActionType.Action:
						DebugAction_NewTmp(debugActionOption.label, debugActionOption.action, highlight);
						break;
					case DebugActionType.ToolMap:
						DebugToolMap_NewTmp(debugActionOption.label, debugActionOption.action, highlight);
						break;
					case DebugActionType.ToolMapForPawns:
						DebugToolMapForPawns_NewTmp(debugActionOption.label, debugActionOption.pawnAction, highlight);
						break;
					case DebugActionType.ToolWorld:
						DebugToolWorld_NewTmp(debugActionOption.label, debugActionOption.action, highlight);
						break;
					}
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
			for (int i = 0; i < debugActions.Count; i++)
			{
				int num = (highlightedIndex + i + 1) % debugActions.Count;
				if (FilterAllows(debugActions[num].label))
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
					DebugActionSelected(highlightedIndex);
				}
				Event.current.Use();
			}
		}

		private void DebugActionSelected(int index)
		{
			DebugActionOption element = debugActions[index];
			switch (element.actionType)
			{
			case DebugActionType.Action:
				element.action();
				break;
			case DebugActionType.ToolMap:
				DebugTools.curTool = new DebugTool(element.label, element.action);
				break;
			case DebugActionType.ToolMapForPawns:
				DebugTools.curTool = new DebugTool(element.label, delegate
				{
					if (UI.MouseCell().InBounds(Find.CurrentMap))
					{
						foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
							where t is Pawn
							select t).Cast<Pawn>().ToList())
						{
							element.pawnAction(item);
						}
					}
				});
				break;
			case DebugActionType.ToolWorld:
				if (WorldRendererUtility.WorldRenderedNow)
				{
					DebugTools.curTool = new DebugTool(element.label, element.action);
				}
				break;
			}
		}
	}
}
