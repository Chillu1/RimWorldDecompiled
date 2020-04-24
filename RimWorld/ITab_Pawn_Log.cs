using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Log : ITab
	{
		public const float Width = 630f;

		[TweakValue("Interface", 0f, 1000f)]
		private static float ShowAllX = 60f;

		[TweakValue("Interface", 0f, 1000f)]
		private static float ShowAllWidth = 100f;

		[TweakValue("Interface", 0f, 1000f)]
		private static float ShowCombatX = 445f;

		[TweakValue("Interface", 0f, 1000f)]
		private static float ShowCombatWidth = 115f;

		[TweakValue("Interface", 0f, 1000f)]
		private static float ShowSocialX = 330f;

		[TweakValue("Interface", 0f, 1000f)]
		private static float ShowSocialWidth = 105f;

		[TweakValue("Interface", 0f, 20f)]
		private static float ToolbarHeight = 2f;

		[TweakValue("Interface", 0f, 100f)]
		private static float ButtonOffset = 60f;

		public bool showAll;

		public bool showCombat = true;

		public bool showSocial = true;

		public LogEntry logSeek;

		public ITab_Pawn_Log_Utility.LogDrawData data = new ITab_Pawn_Log_Utility.LogDrawData();

		public List<ITab_Pawn_Log_Utility.LogLineDisplayable> cachedLogDisplay;

		public int cachedLogDisplayLastTick = -1;

		public int cachedLogPlayLastTick = -1;

		private Pawn cachedLogForPawn;

		private Vector2 scrollPosition;

		private Pawn SelPawnForCombatInfo
		{
			get
			{
				if (base.SelPawn != null)
				{
					return base.SelPawn;
				}
				Corpse corpse = base.SelThing as Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				throw new InvalidOperationException("Social tab on non-pawn non-corpse " + base.SelThing);
			}
		}

		public ITab_Pawn_Log()
		{
			size = new Vector2(630f, 510f);
			labelKey = "TabLog";
		}

		protected override void FillTab()
		{
			Pawn selPawnForCombatInfo = SelPawnForCombatInfo;
			Rect rect = new Rect(0f, 0f, size.x, size.y);
			Rect rect2 = new Rect(ShowAllX, ToolbarHeight, ShowAllWidth, 24f);
			bool flag = showAll;
			Widgets.CheckboxLabeled(rect2, "ShowAll".Translate(), ref showAll);
			if (flag != showAll)
			{
				cachedLogDisplay = null;
			}
			Rect rect3 = new Rect(ShowCombatX, ToolbarHeight, ShowCombatWidth, 24f);
			bool flag2 = showCombat;
			Widgets.CheckboxLabeled(rect3, "ShowCombat".Translate(), ref showCombat);
			if (flag2 != showCombat)
			{
				cachedLogDisplay = null;
			}
			Rect rect4 = new Rect(ShowSocialX, ToolbarHeight, ShowSocialWidth, 24f);
			bool flag3 = showSocial;
			Widgets.CheckboxLabeled(rect4, "ShowSocial".Translate(), ref showSocial);
			if (flag3 != showSocial)
			{
				cachedLogDisplay = null;
			}
			if (cachedLogDisplay == null || cachedLogDisplayLastTick != selPawnForCombatInfo.records.LastBattleTick || cachedLogPlayLastTick != Find.PlayLog.LastTick || cachedLogForPawn != selPawnForCombatInfo)
			{
				cachedLogDisplay = ITab_Pawn_Log_Utility.GenerateLogLinesFor(selPawnForCombatInfo, showAll, showCombat, showSocial).ToList();
				cachedLogDisplayLastTick = selPawnForCombatInfo.records.LastBattleTick;
				cachedLogPlayLastTick = Find.PlayLog.LastTick;
				cachedLogForPawn = selPawnForCombatInfo;
			}
			Rect rect5 = new Rect(rect.width - ButtonOffset, 0f, 18f, 24f);
			if (Widgets.ButtonImage(rect5, TexButton.Copy))
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ITab_Pawn_Log_Utility.LogLineDisplayable item in cachedLogDisplay)
				{
					item.AppendTo(stringBuilder);
				}
				GUIUtility.systemCopyBuffer = stringBuilder.ToString();
			}
			TooltipHandler.TipRegionByKey(rect5, "CopyLogTip");
			rect.yMin = 24f;
			rect = rect.ContractedBy(10f);
			float width = rect.width - 16f - 10f;
			float num = 0f;
			foreach (ITab_Pawn_Log_Utility.LogLineDisplayable item2 in cachedLogDisplay)
			{
				if (item2.Matches(logSeek))
				{
					scrollPosition.y = num - (item2.GetHeight(width) + rect.height) / 2f;
				}
				num += item2.GetHeight(width);
			}
			logSeek = null;
			if (num > 0f)
			{
				Rect viewRect = new Rect(0f, 0f, rect.width - 16f, num);
				data.StartNewDraw();
				Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
				float num2 = 0f;
				foreach (ITab_Pawn_Log_Utility.LogLineDisplayable item3 in cachedLogDisplay)
				{
					item3.Draw(num2, width, data);
					num2 += item3.GetHeight(width);
				}
				Widgets.EndScrollView();
			}
			else
			{
				Text.Anchor = TextAnchor.MiddleCenter;
				GUI.color = Color.grey;
				Widgets.Label(new Rect(0f, 0f, size.x, size.y), "(" + "NoRecentEntries".Translate() + ")");
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
		}

		public void SeekTo(LogEntry entry)
		{
			logSeek = entry;
		}

		public void Highlight(LogEntry entry)
		{
			data.highlightEntry = entry;
			data.highlightIntensity = 1f;
		}

		public override void Notify_ClearingAllMapsMemory()
		{
			base.Notify_ClearingAllMapsMemory();
			cachedLogForPawn = null;
		}
	}
}
