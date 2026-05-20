using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.Sound;

public static class MouseoverSounds
{
	private struct MouseoverRegionCall
	{
		public bool mouseIsOver;

		public Rect rect;

		public SoundDef sound;

		public bool IsValid => rect.x >= 0f;

		public static MouseoverRegionCall Invalid => new MouseoverRegionCall
		{
			rect = new Rect(-1000f, -1000f, 0f, 0f)
		};

		public bool Matches(MouseoverRegionCall other)
		{
			return rect.Equals(other.rect);
		}

		public override string ToString()
		{
			if (!IsValid)
			{
				return "(Invalid)";
			}
			Rect rect = this.rect;
			return "(rect=" + rect.ToString() + (mouseIsOver ? "mouseIsOver" : "") + ")";
		}
	}

	private static List<MouseoverRegionCall> frameCalls = new List<MouseoverRegionCall>();

	private static int lastUsedCallInd = -1;

	private static MouseoverRegionCall lastUsedCall;

	private static int forceSilenceUntilFrame = -1;

	public static void SilenceForNextFrame()
	{
		forceSilenceUntilFrame = Time.frameCount + 1;
	}

	public static void DoRegion(Rect rect)
	{
		DoRegion(rect, SoundDefOf.Mouseover_Standard);
	}

	public static void DoRegion(Rect rect, SoundDef sound)
	{
		if (sound != null && Event.current.type == EventType.Repaint)
		{
			Rect rect2 = new Rect(GUIUtility.GUIToScreenPoint(rect.position), rect.size);
			MouseoverRegionCall item = new MouseoverRegionCall
			{
				rect = rect2,
				sound = sound,
				mouseIsOver = Mouse.IsOver(rect)
			};
			frameCalls.Add(item);
		}
	}

	public static void ResolveFrame()
	{
		for (int i = 0; i < frameCalls.Count; i++)
		{
			if (frameCalls[i].mouseIsOver)
			{
				if (lastUsedCallInd != i && !frameCalls[i].Matches(lastUsedCall) && forceSilenceUntilFrame < Time.frameCount)
				{
					frameCalls[i].sound.PlayOneShotOnCamera();
				}
				lastUsedCallInd = i;
				lastUsedCall = frameCalls[i];
				frameCalls.Clear();
				return;
			}
		}
		lastUsedCall = MouseoverRegionCall.Invalid;
		lastUsedCallInd = -1;
		frameCalls.Clear();
	}
}
