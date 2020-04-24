using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class UIHighlighter
	{
		private static List<Pair<string, int>> liveTags = new List<Pair<string, int>>();

		private const float PulseFrequency = 1.2f;

		private const float PulseAmplitude = 0.7f;

		private static readonly Texture2D TutorHighlightAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/TutorHighlightAtlas");

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
					Rect rect2 = rect.ContractedBy(-10f);
					GUI.color = new Color(1f, 1f, 1f, Pulser.PulseBrightness(1.2f, 0.7f));
					Widgets.DrawAtlas(rect2, TutorHighlightAtlas);
					GUI.color = Color.white;
				}
			}
		}

		public static void UIHighlighterUpdate()
		{
			liveTags.RemoveAll((Pair<string, int> pair) => Time.frameCount > pair.Second + 1);
		}
	}
}
