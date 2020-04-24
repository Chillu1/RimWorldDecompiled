using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class ContentSourceUtility
	{
		public const float IconSize = 24f;

		private static readonly Texture2D ContentSourceIcon_OfficialModsFolder = ContentFinder<Texture2D>.Get("UI/Icons/ContentSources/OfficialModsFolder");

		private static readonly Texture2D ContentSourceIcon_ModsFolder = ContentFinder<Texture2D>.Get("UI/Icons/ContentSources/ModsFolder");

		private static readonly Texture2D ContentSourceIcon_SteamWorkshop = ContentFinder<Texture2D>.Get("UI/Icons/ContentSources/SteamWorkshop");

		public static Texture2D GetIcon(this ContentSource s)
		{
			switch (s)
			{
			case ContentSource.Undefined:
				return BaseContent.BadTex;
			case ContentSource.OfficialModsFolder:
				return ContentSourceIcon_OfficialModsFolder;
			case ContentSource.ModsFolder:
				return ContentSourceIcon_ModsFolder;
			case ContentSource.SteamWorkshop:
				return ContentSourceIcon_SteamWorkshop;
			default:
				throw new NotImplementedException();
			}
		}

		public static void DrawContentSource(Rect r, ContentSource source, Action clickAction = null)
		{
			Rect rect = new Rect(r.x, r.y + r.height / 2f - 12f, 24f, 24f);
			GUI.DrawTexture(rect, source.GetIcon());
			if (Mouse.IsOver(rect))
			{
				TooltipHandler.TipRegion(rect, () => "Source".Translate() + ": " + source.HumanLabel(), (int)(r.x + r.y * 56161f));
				Widgets.DrawHighlight(rect);
			}
			if (clickAction != null && Widgets.ButtonInvisible(rect))
			{
				clickAction();
			}
		}

		public static string HumanLabel(this ContentSource s)
		{
			return ("ContentSource_" + s.ToString()).Translate();
		}
	}
}
