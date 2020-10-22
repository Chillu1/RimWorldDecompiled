using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ITab_Pawn_Log_Utility
	{
		public class LogDrawData
		{
			public bool alternatingBackground;

			public LogEntry highlightEntry;

			public float highlightIntensity;

			public void StartNewDraw()
			{
				alternatingBackground = false;
			}
		}

		public abstract class LogLineDisplayable
		{
			private float cachedHeight = -1f;

			public float GetHeight(float width)
			{
				if (cachedHeight == -1f)
				{
					cachedHeight = GetHeight_Worker(width);
				}
				return cachedHeight;
			}

			public abstract float GetHeight_Worker(float width);

			public abstract void Draw(float position, float width, LogDrawData data);

			public abstract void AppendTo(StringBuilder sb);

			public virtual bool Matches(LogEntry log)
			{
				return false;
			}
		}

		public class LogLineDisplayableHeader : LogLineDisplayable
		{
			private TaggedString text;

			public LogLineDisplayableHeader(TaggedString text)
			{
				this.text = text;
			}

			public override float GetHeight_Worker(float width)
			{
				GameFont font = Text.Font;
				Text.Font = GameFont.Medium;
				float result = Text.CalcHeight(text, width);
				Text.Font = font;
				return result;
			}

			public override void Draw(float position, float width, LogDrawData data)
			{
				Text.Font = GameFont.Medium;
				Widgets.Label(new Rect(0f, position, width, GetHeight(width)), text);
				Text.Font = GameFont.Small;
			}

			public override void AppendTo(StringBuilder sb)
			{
				sb.AppendLine("--    " + text);
			}
		}

		public class LogLineDisplayableLog : LogLineDisplayable
		{
			private LogEntry log;

			private Pawn pawn;

			public LogLineDisplayableLog(LogEntry log, Pawn pawn)
			{
				this.log = log;
				this.pawn = pawn;
			}

			public override float GetHeight_Worker(float width)
			{
				float width2 = width - 29f;
				return Mathf.Max(26f, log.GetTextHeight(pawn, width2));
			}

			public override void Draw(float position, float width, LogDrawData data)
			{
				float height = GetHeight(width);
				float width2 = width - 29f;
				Rect rect = new Rect(0f, position, width, height);
				if (log == data.highlightEntry)
				{
					Widgets.DrawRectFast(rect, new Color(1f, 1f, 1f, HighlightAlpha * data.highlightIntensity));
					data.highlightIntensity = Mathf.Max(0f, data.highlightIntensity - Time.deltaTime / HighlightDuration);
				}
				else if (data.alternatingBackground)
				{
					Widgets.DrawRectFast(rect, new Color(1f, 1f, 1f, AlternateAlpha));
				}
				data.alternatingBackground = !data.alternatingBackground;
				TaggedString label = log.ToGameStringFromPOV(pawn);
				Widgets.Label(new Rect(29f, position, width2, height), label);
				Texture2D texture2D = log.IconFromPOV(pawn);
				if (texture2D != null)
				{
					GUI.DrawTexture(new Rect(0f, position + (height - 26f) / 2f, 26f, 26f), texture2D);
				}
				if (Mouse.IsOver(rect))
				{
					TooltipHandler.TipRegion(rect, () => log.GetTipString(), 613261 + log.LogID * 2063);
					Widgets.DrawHighlight(rect);
				}
				if (Widgets.ButtonInvisible(rect, log.CanBeClickedFromPOV(pawn)))
				{
					log.ClickedFromPOV(pawn);
				}
				if (DebugViewSettings.logCombatLogMouseover && Mouse.IsOver(rect))
				{
					log.ToGameStringFromPOV(pawn, forceLog: true);
				}
			}

			public override void AppendTo(StringBuilder sb)
			{
				sb.AppendLine(log.ToGameStringFromPOV(pawn));
			}

			public override bool Matches(LogEntry log)
			{
				return log == this.log;
			}
		}

		public class LogLineDisplayableGap : LogLineDisplayable
		{
			private float height;

			public LogLineDisplayableGap(float height)
			{
				this.height = height;
			}

			public override float GetHeight_Worker(float width)
			{
				return height;
			}

			public override void Draw(float position, float width, LogDrawData data)
			{
			}

			public override void AppendTo(StringBuilder sb)
			{
				sb.AppendLine();
			}
		}

		[TweakValue("Interface", 0f, 1f)]
		private static float AlternateAlpha = 0.03f;

		[TweakValue("Interface", 0f, 1f)]
		private static float HighlightAlpha = 0.2f;

		[TweakValue("Interface", 0f, 10f)]
		private static float HighlightDuration = 4f;

		[TweakValue("Interface", 0f, 30f)]
		private static float BattleBottomPadding = 20f;

		public static IEnumerable<LogLineDisplayable> GenerateLogLinesFor(Pawn pawn, bool showAll, bool showCombat, bool showSocial)
		{
			LogEntry[] nonCombatLines = (showSocial ? Find.PlayLog.AllEntries.Where((LogEntry e) => e.Concerns(pawn)).ToArray() : new LogEntry[0]);
			int nonCombatIndex = 0;
			Battle currentBattle = null;
			if (showCombat)
			{
				bool atTop = true;
				foreach (Battle battle in Find.BattleLog.Battles)
				{
					if (!battle.Concerns(pawn))
					{
						continue;
					}
					foreach (LogEntry entry in battle.Entries)
					{
						if (!entry.Concerns(pawn) || (!showAll && !entry.ShowInCompactView()))
						{
							continue;
						}
						while (nonCombatIndex < nonCombatLines.Length && nonCombatLines[nonCombatIndex].Age < entry.Age)
						{
							if (currentBattle != null && currentBattle != battle)
							{
								yield return new LogLineDisplayableGap(BattleBottomPadding);
								currentBattle = null;
							}
							yield return new LogLineDisplayableLog(nonCombatLines[nonCombatIndex++], pawn);
							atTop = false;
						}
						if (currentBattle != battle)
						{
							if (!atTop)
							{
								yield return new LogLineDisplayableGap(BattleBottomPadding);
							}
							yield return new LogLineDisplayableHeader(battle.GetName());
							currentBattle = battle;
							atTop = false;
						}
						yield return new LogLineDisplayableLog(entry, pawn);
					}
				}
			}
			while (nonCombatIndex < nonCombatLines.Length)
			{
				if (currentBattle != null)
				{
					yield return new LogLineDisplayableGap(BattleBottomPadding);
					currentBattle = null;
				}
				yield return new LogLineDisplayableLog(nonCombatLines[nonCombatIndex++], pawn);
			}
		}
	}
}
