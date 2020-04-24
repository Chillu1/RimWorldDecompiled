using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

		public Dialog_DebugOutputMenu()
		{
			forcePause = true;
			foreach (Type allType in GenTypes.AllTypes)
			{
				MethodInfo[] methods = allType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.TryGetAttribute(out DebugOutputAttribute customAttribute))
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
			string b = null;
			foreach (DebugOutputOption debugOutput in debugOutputs)
			{
				if (debugOutput.category != b)
				{
					DoLabel(debugOutput.category);
					b = debugOutput.category;
				}
				Log.openOnMessage = true;
				try
				{
					DebugAction(debugOutput.label, debugOutput.action);
				}
				finally
				{
					Log.openOnMessage = false;
				}
			}
		}
	}
}
