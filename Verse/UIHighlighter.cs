using System.Collections.Generic;
using UnityEngine;

namespace Verse;

[StaticConstructorOnStartup]
public static class UIHighlighter
{
	private static List<Pair<string, int>> liveTags = new List<Pair<string, int>>();

	private const float PulseFrequency = 1.2f;

	private const float PulseAmplitude = 0.7f;

	public static void HighlightTag(string tag)
	{
		if (Event.current.type != EventType.Repaint || tag.NullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < liveTags.Count; i++)
		{
			if (liveTags[i].First == tag && liveTags[i].Second == Time.frameCount)
			{
				return;
			}
		}
		liveTags.Add(new Pair<string, int>(tag, Time.frameCount));
	}

	public static void HighlightOpportunity(Rect rect, string tag)
	{
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		for (int i = 0; i < liveTags.Count; i++)
		{
			Pair<string, int> pair = liveTags[i];
			if (tag == pair.First && Time.frameCount == pair.Second + 1)
			{
				Widgets.DrawTextHighlight(rect, 0f, Color.white.ToTransparent(Pulser.PulseBrightness(1.2f, 0.7f)));
			}
		}
	}

	public static void UIHighlighterUpdate()
	{
		liveTags.RemoveAll((Pair<string, int> pair) => Time.frameCount > pair.Second + 1);
	}
}
