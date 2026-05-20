using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public static class PawnPortraitIconsDrawer
{
	private struct PawnPortraitIcon
	{
		public Color color;

		public Texture2D icon;

		public string tooltip;
	}

	private static Texture2D prisonerIcon = ContentFinder<Texture2D>.Get("UI/Icons/Prisoner");

	private static Texture2D sleepingIcon = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Sleeping");

	private static Texture2D slaveryIcon;

	private static List<PawnPortraitIcon> tmpPortraitIcons = new List<PawnPortraitIcon>();

	private static Texture2D SlaveryIcon
	{
		get
		{
			if (slaveryIcon == null)
			{
				slaveryIcon = ContentFinder<Texture2D>.Get("UI/Icons/Slavery");
			}
			return slaveryIcon;
		}
	}

	private static void CalculatePawnPortraitIcons(Pawn pawn, bool required, bool showIdeoIcon)
	{
		using (new ProfilerBlock("CalculatePawnPortraitIcons"))
		{
			tmpPortraitIcons.Clear();
			Ideo ideo = pawn.Ideo;
			if (required)
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = Color.white,
					icon = IdeoUIUtility.LockedTex,
					tooltip = "Required".Translate()
				});
			}
			if (!ModsConfig.IdeologyActive || ideo == null)
			{
				return;
			}
			if (!Find.IdeoManager.classicMode && showIdeoIcon)
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = ideo.Color,
					icon = ideo.Icon,
					tooltip = ideo.memberName
				});
				Precept_Role role = ideo.GetRole(pawn);
				if (role != null)
				{
					tmpPortraitIcons.Add(new PawnPortraitIcon
					{
						color = ideo.Color,
						icon = role.Icon,
						tooltip = role.TipLabel
					});
				}
				GUI.color = Color.white;
			}
			Faction homeFaction = pawn.HomeFaction;
			if (homeFaction != null && !homeFaction.IsPlayer && !homeFaction.Hidden)
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = homeFaction.Color,
					icon = homeFaction.def.FactionIcon,
					tooltip = "Faction".Translate() + ": " + homeFaction.Name + "\n" + homeFaction.def.LabelCap
				});
			}
			if (pawn.IsSlave)
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = Color.white,
					icon = SlaveryIcon,
					tooltip = "RitualBeginSlaveDesc".Translate()
				});
			}
			if (pawn.IsPrisoner)
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = Color.white,
					icon = prisonerIcon,
					tooltip = null
				});
			}
			if (!pawn.Awake())
			{
				tmpPortraitIcons.Add(new PawnPortraitIcon
				{
					color = Color.white,
					icon = sleepingIcon,
					tooltip = "RitualBeginSleepingDesc".Translate(pawn)
				});
			}
		}
	}

	public static void DrawPawnPortraitIcons(Rect portraitRect, Pawn p, bool required, bool grayedOut, ref float curX, ref float curY, float iconSize, bool showIdeoIcon, out bool tooltipActive)
	{
		CalculatePawnPortraitIcons(p, required, showIdeoIcon);
		tooltipActive = false;
		foreach (PawnPortraitIcon tmpPortraitIcon in tmpPortraitIcons)
		{
			PawnPortraitIcon localIcon = tmpPortraitIcon;
			Rect rect = new Rect(curX - iconSize, curY - iconSize, iconSize, iconSize);
			curX -= iconSize;
			if (curX - iconSize < portraitRect.x)
			{
				curX = portraitRect.xMax;
				curY -= iconSize + 2f;
			}
			GUI.color = (grayedOut ? tmpPortraitIcon.color.SaturationChanged(0f) : tmpPortraitIcon.color);
			Widgets.DrawTextureFitted(rect, tmpPortraitIcon.icon, 1f);
			GUI.color = Color.white;
			if (tmpPortraitIcon.tooltip != null)
			{
				if (Mouse.IsOver(rect))
				{
					tooltipActive = true;
				}
				TooltipHandler.TipRegion(rect, () => localIcon.tooltip, tmpPortraitIcon.icon.GetInstanceID() ^ p.thingIDNumber);
			}
		}
	}
}
