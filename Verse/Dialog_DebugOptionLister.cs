using RimWorld.Planet;
using System;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public abstract class Dialog_DebugOptionLister : Dialog_OptionLister
	{
		private const float DebugOptionsGap = 7f;

		public Dialog_DebugOptionLister()
		{
			forcePause = true;
		}

		protected bool DebugAction(string label, Action action)
		{
			bool result = false;
			if (!FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			if (listing.ButtonDebug(label))
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

		protected void DebugToolMap(string label, Action toolAction)
		{
			if (!WorldRendererUtility.WorldRenderedNow)
			{
				if (!FilterAllows(label))
				{
					GUI.color = new Color(1f, 1f, 1f, 0.3f);
				}
				if (listing.ButtonDebug(label))
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

		protected void DebugToolMapForPawns(string label, Action<Pawn> pawnAction)
		{
			DebugToolMap(label, delegate
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
			});
		}

		protected void DebugToolWorld(string label, Action toolAction)
		{
			if (WorldRendererUtility.WorldRenderedNow)
			{
				if (!FilterAllows(label))
				{
					GUI.color = new Color(1f, 1f, 1f, 0.3f);
				}
				if (listing.ButtonDebug(label))
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

		protected void CheckboxLabeledDebug(string label, ref bool checkOn)
		{
			if (!FilterAllows(label))
			{
				GUI.color = new Color(1f, 1f, 1f, 0.3f);
			}
			listing.LabelCheckboxDebug(label, ref checkOn);
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
	}
}
