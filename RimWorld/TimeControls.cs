using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class TimeControls
	{
		public static readonly Vector2 TimeButSize = new Vector2(32f, 24f);

		private static readonly TimeSpeed[] CachedTimeSpeedValues = (TimeSpeed[])Enum.GetValues(typeof(TimeSpeed));

		private static void PlaySoundOf(TimeSpeed speed)
		{
			SoundDef soundDef = null;
			switch (speed)
			{
			case TimeSpeed.Paused:
				soundDef = SoundDefOf.Clock_Stop;
				break;
			case TimeSpeed.Normal:
				soundDef = SoundDefOf.Clock_Normal;
				break;
			case TimeSpeed.Fast:
				soundDef = SoundDefOf.Clock_Fast;
				break;
			case TimeSpeed.Superfast:
				soundDef = SoundDefOf.Clock_Superfast;
				break;
			case TimeSpeed.Ultrafast:
				soundDef = SoundDefOf.Clock_Superfast;
				break;
			}
			soundDef?.PlayOneShotOnCamera();
		}

		public static void DoTimeControlsGUI(Rect timerRect)
		{
			TickManager tickManager = Find.TickManager;
			GUI.BeginGroup(timerRect);
			Rect rect = new Rect(0f, 0f, TimeButSize.x, TimeButSize.y);
			for (int i = 0; i < CachedTimeSpeedValues.Length; i++)
			{
				TimeSpeed timeSpeed = CachedTimeSpeedValues[i];
				if (timeSpeed == TimeSpeed.Ultrafast)
				{
					continue;
				}
				if (Widgets.ButtonImage(rect, TexButton.SpeedButtonTextures[(uint)timeSpeed]))
				{
					if (timeSpeed == TimeSpeed.Paused)
					{
						tickManager.TogglePaused();
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
					}
					else
					{
						tickManager.CurTimeSpeed = timeSpeed;
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					}
					PlaySoundOf(tickManager.CurTimeSpeed);
				}
				if (tickManager.CurTimeSpeed == timeSpeed)
				{
					GUI.DrawTexture(rect, TexUI.HighlightTex);
				}
				rect.x += rect.width;
			}
			if (Find.TickManager.slower.ForcedNormalSpeed)
			{
				Widgets.DrawLineHorizontal(rect.width * 2f, rect.height / 2f, rect.width * 2f);
			}
			GUI.EndGroup();
			GenUI.AbsorbClicksInRect(timerRect);
			UIHighlighter.HighlightOpportunity(timerRect, "TimeControls");
			if (Event.current.type != EventType.KeyDown)
			{
				return;
			}
			if (KeyBindingDefOf.TogglePause.KeyDownEvent)
			{
				Find.TickManager.TogglePaused();
				PlaySoundOf(Find.TickManager.CurTimeSpeed);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
				Event.current.Use();
			}
			if (!Find.WindowStack.WindowsForcePause)
			{
				if (KeyBindingDefOf.TimeSpeed_Normal.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
					PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (KeyBindingDefOf.TimeSpeed_Fast.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Fast;
					PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
				if (KeyBindingDefOf.TimeSpeed_Superfast.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
					PlaySoundOf(Find.TickManager.CurTimeSpeed);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
					Event.current.Use();
				}
			}
			if (Prefs.DevMode)
			{
				if (KeyBindingDefOf.TimeSpeed_Ultrafast.KeyDownEvent)
				{
					Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
					PlaySoundOf(Find.TickManager.CurTimeSpeed);
					Event.current.Use();
				}
				if (KeyBindingDefOf.Dev_TickOnce.KeyDownEvent && tickManager.CurTimeSpeed == TimeSpeed.Paused)
				{
					tickManager.DoSingleTick();
					SoundDefOf.Clock_Stop.PlayOneShotOnCamera();
				}
			}
		}
	}
}
