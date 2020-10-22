using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse
{
	public abstract class Dialog_DebugOptionLister : Dialog_OptionLister
	{
		protected int currentHighlightIndex;

		protected int prioritizedHighlightedIndex;

		private const float DebugOptionsGap = 7f;

		protected virtual int HighlightedIndex => -1;

		public Dialog_DebugOptionLister()
		{
			forcePause = true;
		}

		protected bool DebugAction_NewTmp(string label, Action action, bool highlight)
		{
			bool result = false;
			if (!FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			if (listing.ButtonDebug_NewTmp(label, highlight))
			{
				Close();
				action();
				result = true;
			}
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight += 24f;
			}
			return result;
		}

		protected void DebugToolMap_NewTmp(string label, Action toolAction, bool highlight)
		{
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				if (!FilterAllows(label))
				{
					GUI.color = new Color(1f, 1f, 1f, 0.3f);
				}
				if (listing.ButtonDebug_NewTmp(label, highlight))
				{
					Close();
					DebugTools.curTool = new DebugTool(label, toolAction);
				}
				GUI.color = Color.white;
				if (Event.current.type == EventType.Layout)
				{
					totalOptionsHeight += 24f;
				}
			}
		}

		protected void DebugToolMapForPawns_NewTmp(string label, Action<Pawn> pawnAction, bool highlight)
		{
			DebugToolMap_NewTmp(label, delegate
			{
				if (UI.MouseCell().InBounds(Find.CurrentMap))
				{
					foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>().ToList())
					{
						pawnAction(item);
					}
				}
			}, highlight);
		}

		protected void DebugToolWorld_NewTmp(string label, Action toolAction, bool highlight)
		{
			if (WorldRendererUtility.WorldRenderedNow)
			{
				if (!FilterAllows(label))
				{
					GUI.color = new Color(1f, 1f, 1f, 0.3f);
				}
				if (listing.ButtonDebug_NewTmp(label, highlight))
				{
					Close();
					DebugTools.curTool = new DebugTool(label, toolAction);
				}
				GUI.color = Color.white;
				if (Event.current.type == EventType.Layout)
				{
					totalOptionsHeight += 24f;
				}
			}
		}

		protected override void DoListingItems()
		{
			if (KeyBindingDefOf.Dev_ChangeSelectedDebugAction.IsDownEvent)
			{
				ChangeHighlightedOption();
			}
		}

		protected virtual void ChangeHighlightedOption()
		{
		}

		[Obsolete("Only used for mod compatibility")]
		protected void CheckboxLabeledDebug(string label, ref bool checkOn)
		{
			CheckboxLabeledDebug(label, ref checkOn);
		}

		protected void CheckboxLabeledDebug_NewTmp(string label, ref bool checkOn, bool highlighted)
		{
			if (!FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			listing.LabelCheckboxDebug_NewTmp(label, ref checkOn, highlighted);
			GUI.color = Color.white;
			if (Event.current.type == EventType.Layout)
			{
				totalOptionsHeight += 24f;
			}
		}

		protected void DoLabel(string label)
		{
			Text.Font = GameFont.Small;
			listing.Label(label);
			totalOptionsHeight += Text.CalcHeight(label, 300f) + 2f;
		}

		protected void DoGap()
		{
			listing.Gap(7f);
			totalOptionsHeight += 7f;
		}

		[Obsolete("Only used for mod compatibility")]
		protected bool DebugAction(string label, Action action)
		{
			return DebugAction_NewTmp(label, action, highlight: false);
		}

		[Obsolete("Only used for mod compatibility")]
		protected void DebugToolMap(string label, Action toolAction)
		{
			DebugToolMap_NewTmp(label, toolAction, highlight: false);
		}

		[Obsolete("Only used for mod compatibility")]
		protected void DebugToolMapForPawns(string label, Action<Pawn> pawnAction)
		{
			DebugToolMapForPawns_NewTmp(label, pawnAction, highlight: false);
		}

		[Obsolete("Only used for mod compatibility")]
		protected void DebugToolWorld(string label, Action toolAction)
		{
			DebugToolWorld_NewTmp(label, toolAction, highlight: false);
		}
	}
}
