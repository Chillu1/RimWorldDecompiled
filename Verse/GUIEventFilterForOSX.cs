using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class GUIEventFilterForOSX
	{
		private static List<Event> eventsThisFrame = new List<Event>();

		private static int lastRecordedFrame = -1;

		public static void CheckRejectGUIEvent()
		{
			if (UnityData.platform != RuntimePlatform.OSXPlayer || (Event.current.type != 0 && Event.current.type != EventType.MouseUp))
			{
				return;
			}
			if (Time.frameCount != lastRecordedFrame)
			{
				eventsThisFrame.Clear();
				lastRecordedFrame = Time.frameCount;
			}
			for (int i = 0; i < eventsThisFrame.Count; i++)
			{
				if (EventsAreEquivalent(eventsThisFrame[i], Event.current))
				{
					RejectEvent();
				}
			}
			eventsThisFrame.Add(Event.current);
		}

		private static bool EventsAreEquivalent(Event A, Event B)
		{
			if (A.button == B.button && A.keyCode == B.keyCode)
			{
				return A.type == B.type;
			}
			return false;
		}

		private static void RejectEvent()
		{
			if (DebugViewSettings.logInput)
			{
				Log.Message("Frame " + Time.frameCount + ": REJECTED " + Event.current.ToStringFull());
			}
			Event.current.Use();
		}
	}
}
