using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		public Dialog_DebugActionsMenu()
		{
			forcePause = true;
			foreach (Type allType in GenTypes.AllTypes)
			{
				MethodInfo[] methods = allType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.TryGetAttribute(out DebugActionAttribute customAttribute))
					{
						GenerateCacheForMethod(methodInfo, customAttribute);
					}
					if (methodInfo.TryGetAttribute(out DebugActionYielderAttribute _))
					{
						foreach (DebugActionOption item in (IEnumerable<DebugActionOption>)methodInfo.Invoke(null, null))
						{
							debugActions.Add(item);
						}
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
				string text = string.IsNullOrEmpty(attribute.name) ? GenText.SplitCamelCase(method.Name) : attribute.name;
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
					item.pawnAction = (Delegate.CreateDelegate(typeof(Action<Pawn>), method) as Action<Pawn>);
				}
				else
				{
					item.action = (Delegate.CreateDelegate(typeof(Action), method) as Action);
				}
				debugActions.Add(item);
			}
		}

		protected override void DoListingItems()
		{
			if (KeyBindingDefOf.Dev_ToggleDebugActionsMenu.KeyDownEvent)
			{
				Event.current.Use();
				Close();
			}
			string b = null;
			foreach (DebugActionOption debugAction in debugActions)
			{
				if (debugAction.category != b)
				{
					DoGap();
					DoLabel(debugAction.category);
					b = debugAction.category;
				}
				Log.openOnMessage = true;
				try
				{
					switch (debugAction.actionType)
					{
					case DebugActionType.Action:
						DebugAction(debugAction.label, debugAction.action);
						break;
					case DebugActionType.ToolMap:
						DebugToolMap(debugAction.label, debugAction.action);
						break;
					case DebugActionType.ToolMapForPawns:
						DebugToolMapForPawns(debugAction.label, debugAction.pawnAction);
						break;
					case DebugActionType.ToolWorld:
						DebugToolWorld(debugAction.label, debugAction.action);
						break;
					}
				}
				finally
				{
					Log.openOnMessage = false;
				}
			}
		}
	}
}
