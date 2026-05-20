using System;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld;

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
		SteamDeck.Vibrate();
	}

	private static KeyBindingDef GetBindingFor(TimeSpeed speed)
	{
		return speed switch
		{
			TimeSpeed.Paused => KeyBindingDefOf.TogglePause, 
			TimeSpeed.Normal => KeyBindingDefOf.TimeSpeed_Normal, 
			TimeSpeed.Fast => KeyBindingDefOf.TimeSpeed_Fast, 
			TimeSpeed.Superfast => KeyBindingDefOf.TimeSpeed_Superfast, 
			TimeSpeed.Ultrafast => KeyBindingDefOf.TimeSpeed_Ultrafast, 
			_ => null, 
		};
	}

	public static void DoTimeControlsGUI(Rect timerRect)
	{
		TickManager tickManager = Find.TickManager;
		Widgets.BeginGroup(timerRect);
		Rect rect = new Rect(0f, 0f, TimeButSize.x, TimeButSize.y);
		for (int i = 0; i < CachedTimeSpeedValues.Length; i++)
		{
			TimeSpeed timeSpeed = CachedTimeSpeedValues[i];
			if (timeSpeed == TimeSpeed.Ultrafast)
			{
				continue;
			}
			string arg = KeyPrefs.KeyPrefsData.GetBoundKeyCode(GetBindingFor(timeSpeed), KeyPrefs.BindingSlot.A).ToStringReadable();
			string tooltip = string.Format("{0}: {1}", "HotKeyTip".Translate(), arg);
			if (Widgets.ButtonImage(rect, TexButton.SpeedButtonTextures[(uint)timeSpeed], doMouseoverSound: true, tooltip) && !tickManager.ForcePaused)
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
			if (((!tickManager.ForcePaused) ? tickManager.CurTimeSpeed : TimeSpeed.Paused) == timeSpeed)
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
			rect.x += rect.width;
		}
		if (tickManager.slower.ForcedNormalSpeed)
		{
			Widgets.DrawLineHorizontal(rect.width * 2f, rect.height / 2f, rect.width * 2f);
		}
		if (tickManager.ForcePaused)
		{
			Widgets.DrawLineHorizontal(rect.width, rect.height / 2f, rect.width * 3f);
		}
		Widgets.EndGroup();
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
			if (KeyBindingDefOf.TimeSpeed_Slower.KeyDownEvent && Find.TickManager.CurTimeSpeed != TimeSpeed.Paused)
			{
				Find.TickManager.CurTimeSpeed--;
				PlaySoundOf(Find.TickManager.CurTimeSpeed);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
				Event.current.Use();
			}
			if (KeyBindingDefOf.TimeSpeed_Faster.KeyDownEvent && (int)Find.TickManager.CurTimeSpeed < 3)
			{
				Find.TickManager.CurTimeSpeed++;
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
