using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class MainButtonWorker
{
	public MainButtonDef def;

	private const float CompactModeMargin = 2f;

	private const float IconSize = 32f;

	public virtual float ButtonBarPercent => 0f;

	public virtual bool Disabled
	{
		get
		{
			if (Find.CurrentMap == null && (!def.validWithoutMap || def == MainButtonDefOf.World))
			{
				return true;
			}
			if (Find.WorldRoutePlanner.Active && Find.WorldRoutePlanner.FormingCaravan && (!def.validWithoutMap || def == MainButtonDefOf.World))
			{
				return true;
			}
			if (Find.TilePicker.Active && !Find.TilePicker.AllowEscape && (!def.validWithoutMap || def == MainButtonDefOf.World))
			{
				return true;
			}
			return false;
		}
	}

	public virtual bool Visible
	{
		get
		{
			if (ModsConfig.IdeologyActive && !def.validWithClassicIdeo && Find.IdeoManager.classicMode)
			{
				return false;
			}
			return def.buttonVisible;
		}
	}

	public abstract void Activate();

	public virtual void InterfaceTryActivate()
	{
		if (!TutorSystem.TutorialMode || !def.canBeTutorDenied || Find.MainTabsRoot.OpenTab == def || TutorSystem.AllowAction("MainTab-" + def.defName + "-Open"))
		{
			if (def.closesWorldView && Find.TilePicker.Active && !Find.TilePicker.AllowEscape)
			{
				Messages.Message("MessagePlayerMustSelectTile".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				Activate();
			}
		}
	}

	public virtual void DoButton(Rect rect)
	{
		Text.Font = GameFont.Small;
		string text = def.LabelCap;
		float num = def.LabelCapWidth;
		if (num > rect.width - 2f)
		{
			text = def.ShortenedLabelCap;
			num = def.ShortenedLabelCapWidth;
		}
		if (Disabled)
		{
			Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
			if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
			{
				Event.current.Use();
			}
			return;
		}
		bool flag = num > 0.85f * rect.width - 1f;
		Rect rect2 = rect;
		string label = ((def.Icon == null) ? text : "");
		float textLeftMargin = (flag ? 2f : (-1f));
		if (Widgets.ButtonTextSubtle(rect2, label, ButtonBarPercent, textLeftMargin, SoundDefOf.Mouseover_Category))
		{
			InterfaceTryActivate();
		}
		if (def.Icon != null)
		{
			Vector2 center = rect.center;
			float num2 = 16f;
			if (Mouse.IsOver(rect))
			{
				center += new Vector2(2f, -2f);
			}
			GUI.DrawTexture(new Rect(center.x - num2, center.y - num2, 32f, 32f), def.Icon);
		}
		if (Find.MainTabsRoot.OpenTab != def && !Find.WindowStack.NonImmediateDialogWindowOpen)
		{
			UIHighlighter.HighlightOpportunity(rect, def.cachedHighlightTagClosed);
		}
		if (Mouse.IsOver(rect) && !def.description.NullOrEmpty())
		{
			TooltipHandler.TipRegion(rect, def.LabelCap.Colorize(ColorLibrary.Yellow) + "\n\n" + def.description);
		}
	}
}
