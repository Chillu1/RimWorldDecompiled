using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace Verse;

[StaticConstructorOnStartup]
public static class Messages
{
	private static List<Message> liveMessages = new List<Message>();

	private static int mouseoverMessageIndex = -1;

	public static readonly Vector2 MessagesTopLeftStandard = new Vector2(140f, 16f);

	private const int MessageYInterval = 26;

	private const int MaxLiveMessages = 12;

	public static void Update()
	{
		if (Current.ProgramState == ProgramState.Playing && mouseoverMessageIndex >= 0 && mouseoverMessageIndex < liveMessages.Count)
		{
			liveMessages[mouseoverMessageIndex].lookTargets.TryHighlight();
		}
		mouseoverMessageIndex = -1;
		liveMessages.RemoveAll((Message m) => m.Expired);
	}

	public static void Message(string text, LookTargets lookTargets, MessageTypeDef def, Quest quest, bool historical = true)
	{
		if (AcceptsMessage(text, lookTargets))
		{
			Message(new Message(text.CapitalizeFirst(), def, lookTargets, quest), historical);
		}
	}

	public static void Message(string text, LookTargets lookTargets, MessageTypeDef def, bool historical = true)
	{
		if (AcceptsMessage(text, lookTargets))
		{
			Message(new Message(text.CapitalizeFirst(), def, lookTargets), historical);
		}
	}

	public static void Message(string text, MessageTypeDef def, bool historical = true)
	{
		if (AcceptsMessage(text, TargetInfo.Invalid))
		{
			Message(new Message(text.CapitalizeFirst(), def), historical);
		}
	}

	public static void Message(Message msg, bool historical = true)
	{
		if (AcceptsMessage(msg.text, msg.lookTargets))
		{
			if (historical && Find.Archive != null)
			{
				Find.Archive.Add(msg);
			}
			liveMessages.Add(msg);
			while (liveMessages.Count > 12)
			{
				liveMessages.RemoveAt(0);
			}
			if (msg.def.sound != null)
			{
				msg.def.sound.PlayOneShotOnCamera();
			}
		}
	}

	public static bool IsLive(Message msg)
	{
		return liveMessages.Contains(msg);
	}

	public static void MessagesDoGUI()
	{
		Text.Font = GameFont.Small;
		int xOffset = (int)MessagesTopLeftStandard.x;
		int num = (int)MessagesTopLeftStandard.y;
		if (Current.Game != null && Find.ActiveLesson.ActiveLessonVisible)
		{
			num += (int)Find.ActiveLesson.Current.MessagesYOffset;
		}
		for (int num2 = liveMessages.Count - 1; num2 >= 0; num2--)
		{
			liveMessages[num2].Draw(xOffset, num);
			num += 26;
		}
	}

	public static bool CollidesWithAnyMessage(Rect rect, out float messageAlpha)
	{
		bool result = false;
		float num = 0f;
		for (int i = 0; i < liveMessages.Count; i++)
		{
			Message message = liveMessages[i];
			if (rect.Overlaps(message.lastDrawRect))
			{
				result = true;
				num = Mathf.Max(num, message.Alpha);
			}
		}
		messageAlpha = num;
		return result;
	}

	public static void Clear()
	{
		liveMessages.Clear();
	}

	public static void Notify_LoadedLevelChanged()
	{
		for (int i = 0; i < liveMessages.Count; i++)
		{
			liveMessages[i].lookTargets = null;
		}
	}

	private static bool AcceptsMessage(string text, LookTargets lookTargets)
	{
		if (text.NullOrEmpty())
		{
			return false;
		}
		for (int i = 0; i < liveMessages.Count; i++)
		{
			if (liveMessages[i].text == text && LookTargets.SameTargets(liveMessages[i].lookTargets, lookTargets))
			{
				if (liveMessages[i].startingFrame == RealTime.frameCount)
				{
					return false;
				}
				liveMessages[i].ResetTimer();
				liveMessages[i].Flash();
				liveMessages[i].def.sound.PlayOneShotOnCamera();
				return false;
			}
		}
		return true;
	}

	public static void Notify_Mouseover(Message msg)
	{
		mouseoverMessageIndex = liveMessages.IndexOf(msg);
	}
}
